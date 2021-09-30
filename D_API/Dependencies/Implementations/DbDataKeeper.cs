using D_API.DataContexts;
using D_API.Dependencies.Interfaces;
using D_API.Models.DataKeeper;
using D_API.Types.DataKeeper;
using DiegoG.Utilities.IO;
using DiegoG.Utilities.Measures;
using NeoSmart.AsyncLock;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace D_API.Dependencies.Implementations
{
    public class DbDataKeeper : IAppDataKeeper
    {
        private readonly UserDataContext Db;
        private readonly string HashKey;

        public DbDataKeeper(UserDataContext db, string hashKey)
        {
            Db = db;
            HashKey = hashKey;
        }

        public async Task<bool> CheckExists(Guid userkey, string datakey)
            => (await Db.DataEntries.FindAsync(userkey, datakey))?.ReadAccess.Any(x => x == userkey) is true;

        public async Task<DataOperationResults<double>> Upload(Guid userkey, string datakey, byte[] data, bool overwrite)
        {
            DataEntry? dataEntry;
            UserDataTracker? usageTracker;
            {
                var t = Db.DataEntries.FindAsync(userkey, datakey);
                usageTracker = await Db.UserDataTrackers.FindAsync(userkey);
                dataEntry = await t;
            }

            try
            {
                await usageTracker.ClearOutdatedTransferTrackers();

                var (_, ouq, _, ou) = await usageTracker.GetIfOverTransferQuota();
                if (ouq)
                    return new(DataOpResult.OverTransferQuota, ou);

                double overstorage = 0;
                if ((overstorage = usageTracker.WouldSurpassStorageQuota(data.Length)) > 0)
                    return new(DataOpResult.OverStorageQuota, overstorage);

                if (dataEntry is null)
                    dataEntry = new(userkey, datakey, data, Guid.NewGuid(), new());
                else if (dataEntry.IsImportant && !overwrite)
                    return new(DataOpResult.NoOverwrite, 0);
                //If it does contain the key but the value is null, it should overwrite it

                await usageTracker.AddTracker(new(0, data.Length));
                usageTracker.StorageUsage += data.Length - dataEntry.Size;
                dataEntry.Data = data;

                return new(DataOpResult.Success, 0);
            }
            finally
            {
                await Db.SaveChangesAsync();
            }
        }

        public async Task<DataOperationResults<byte[]?, double>> Download(Guid userkey, string datakey)
        {
            try
            {
                var usageTracker = await Db.UserDataTrackers.FindAsync(userkey);
                await usageTracker.ClearOutdatedTransferTrackers();

                var (odq, _, od, _) = await usageTracker.GetIfOverTransferQuota();
                if (odq)
                    return new(DataOpResult.OverTransferQuota, null, od);

                var dataEntry = await Db.DataEntries.FindAsync(userkey, datakey);

                if (dataEntry is null or { IsImportant: false })
                    return new(DataOpResult.DataDoesNotExist, null, 0);

                await usageTracker.AddTracker(new(dataEntry.Size, 0));
            
                return new(DataOpResult.Success, dataEntry.Data, 0);
            }
            finally
            {
                await Db.SaveChangesAsync();
            }
        }

        public async Task<TransferReport> GetTransferQuota(Guid userkey)
            => (await Db.UserDataTrackers.FindAsync(userkey)).GetTransferQuota();

        public async Task<TransferReport> GetTransferUsage(Guid userkey)
            => await (await Db.UserDataTrackers.FindAsync(userkey)).GetTotalTransferUsage();

        public async Task<double> GetStorageQuota(Guid userkey) 
            => (await Db.UserDataTrackers.FindAsync(userkey)).StorageQuota;

        public async Task<double> GetStorageUsage(Guid userkey) 
            => (await Db.UserDataTrackers.FindAsync(userkey)).StorageUsage;

        public async Task<DataOperationResults<Guid>> GetReadonlyKey(Guid userkey, string datakey) 
            => await Db.DataEntries.FindAsync(userkey, datakey) switch
            {
                null or { IsImportant: false } => new(DataOpResult.DataDoesNotExist, Guid.Empty),
                { ReadOnlyKey: Guid key } => new(DataOpResult.Success, key)
            };

        public async Task<DataOperationResults> AddReaders(Guid userkey, string datakey, params Guid[] readerkeys)
        {
            if (!readerkeys.Any())
                return new(DataOpResult.Success);

            var dt = await Db.DataEntries.FindAsync(userkey, datakey);
            if (dt is null or { IsImportant: false })
                return new(DataOpResult.DataDoesNotExist);

            var set = new HashSet<Guid>(readerkeys.Except(dt.ReadAccess));
            dt.Readers.AddRange(Db.UserHandles.Where(x => set.Contains(x.Key)));
            
            await Db.SaveChangesAsync();
            return new(DataOpResult.Success);
        }

        public async Task<DataOperationResults> RemoveReaders(Guid userkey, string datakey, params Guid[] readerkeys)
        {
            if (!readerkeys.Any())
                return new(DataOpResult.Success);

            var dt = await Db.DataEntries.FindAsync(userkey, datakey);
            if (dt is null or { IsImportant: false })
                return new(DataOpResult.DataDoesNotExist);

            var set = new HashSet<Guid>(readerkeys);
            dt.Readers.RemoveAll(x => set.Contains(x.Key));

            await Db.SaveChangesAsync();
            return new(DataOpResult.Success);
        }

        public async Task<DataOperationResults<byte[]?, double>> DownloadReadonly(Guid userkey, Guid dataKey)
        {
            var usageTrackerTask = Task.Run(async () =>
            {
                var t = await Db.UserDataTrackers.FindAsync(userkey);
                await t.ClearOutdatedTransferTrackers();
                return t;
            });

            var dataEntry = Db.DataEntries.FirstOrDefault(x => x.ReadOnlyKey == dataKey);

            if (dataEntry is null or { IsImportant: false })
                return new(DataOpResult.DataDoesNotExist, null, 0);
            if (!dataEntry.Readers.Any(x => x.Key == userkey)) 
                return new(DataOpResult.DataInaccessible, null, 0);

            var usageTracker = await usageTrackerTask;
            try
            {
                var (odq, _, od, _) = await usageTracker.GetIfOverTransferQuota();
                if (odq)
                    return new(DataOpResult.OverTransferQuota, null, od);

                await usageTracker.AddTracker(new(dataEntry.Size, 0));
                return new(DataOpResult.Success, dataEntry.Data, 0);
            }
            finally
            {
                await Db.SaveChangesAsync();
            }
        }

        public bool EnsureRoot() => DbHelper.InitializeRootUser(Db, HashKey);
        public async Task<UserOperationResults> SetUserQuotas(Guid userkey, double? upload = null, double? download = null, double? storage = null)
        {
            var usageTracker = await Db.UserDataTrackers.FindAsync(userkey);
            usageTracker.DailyTransferUploadQuota = upload ?? usageTracker.DailyTransferUploadQuota;
            usageTracker.DailyTransferDownloadQuota = download ?? usageTracker.DailyTransferDownloadQuota;
            usageTracker.StorageQuota = storage ?? usageTracker.StorageQuota;
            await Db.SaveChangesAsync();
            return new(UserOperationResult.Success);
        }
    }
}
