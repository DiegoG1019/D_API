using D_API.DataContexts;
using D_API.Dependencies.Interfaces;
using D_API.Models.DataKeeper;
using D_API.Models.DataKeeper.DataUsage.Complex;
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
        private static readonly TimeSpan DailyTransferExpiration = TimeSpan.FromDays(1);

        private readonly ClientDataContext Db;

        public DbDataKeeper(ClientDataContext db) => Db = db;

        public async Task<bool> CheckExists(Guid userkey, string datakey)
            => (await Db.DataEntries.FindAsync(userkey, datakey))?.ReadAccess.Any(x => x == userkey) is true;

        public async Task<DataOperationResults<double>> Upload(Guid userkey, string datakey, byte[] data, bool overwrite)
        {
            DataEntry? dataEntry;
            ClientDataTracker? usageTracker;
            {
                var t = Db.DataEntries.FindAsync(userkey, datakey);
                usageTracker = await Db.ClientDataUsages.FindAsync(userkey);
                dataEntry = await t;
            }

            try
            {
                await usageTracker.DailyTransferUsage.ClearOutdatedTrackers();

                var (_, ouq, _, ou) = await usageTracker.GetIfOverTransferQuota();
                if (ouq)
                    return new(DataOpResult.OverTransferQuota, ou);

                double overstorage = 0;
                if ((overstorage = usageTracker.WouldSurpassStorageQuota(data.Length)) > 0)
                    return new(DataOpResult.OverStorageQuota, overstorage);

                if (dataEntry is null)
                    dataEntry = new(userkey, datakey, data, Guid.NewGuid());
                else if (dataEntry.IsImportant && !overwrite)
                    return new(DataOpResult.NoOverwrite, 0);
                    //If it does contain the key but the value is null, it should overwrite it

                usageTracker.DailyTransferUsage.Trackers.Enqueue(new(DateTime.Now, DailyTransferExpiration, 0, data.Length));
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
                var usageTracker = await Db.ClientDataUsages.FindAsync(userkey);
                await usageTracker.DailyTransferUsage.ClearOutdatedTrackers();

                var (odq, _, od, _) = await usageTracker.GetIfOverTransferQuota();
                if (odq)
                    return new(DataOpResult.OverTransferQuota, null, od);

                var dataEntry = await Db.DataEntries.FindAsync(userkey, datakey);

                if (dataEntry is null or { IsImportant: false })
                    return new(DataOpResult.DataDoesNotExist, null, 0);

                usageTracker.DailyTransferUsage.Trackers.Enqueue(new(DateTime.Now, DailyTransferExpiration, dataEntry.Size, 0));
            
                return new(DataOpResult.Success, dataEntry.Data, 0);
            }
            finally
            {
                await Db.SaveChangesAsync();
            }
        }

        public async Task<UsageTracker> GetTransferQuota(Guid userkey)
            => (await Db.ClientDataUsages.FindAsync(userkey)).DailyTransferQuota;

        public async Task<UsageTracker> GetTransferUsage(Guid userkey)
            => await (await Db.ClientDataUsages.FindAsync(userkey)).DailyTransferUsage.GetTotal();

        public async Task<DataMeasure> GetStorageQuota(Guid userkey) 
            => (await Db.ClientDataUsages.FindAsync(userkey)).StorageQuota;

        public async Task<DataMeasure> GetStorageUsage(Guid userkey) 
            => (await Db.ClientDataUsages.FindAsync(userkey)).StorageUsage;

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
            dt.Readers.AddRange(readerkeys.Except(dt.ReadAccess));
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
            dt.Readers.RemoveAll(x => set.Contains(x));

            await Db.SaveChangesAsync();
            return new(DataOpResult.Success);
        }

        public Task<DataOperationResults<byte[]?, double>> DownloadReadonly(Guid userkey, Guid dataId) => throw new NotImplementedException();
    }
}
