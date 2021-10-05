using D_API.Types.DataKeeper;
using DiegoG.Utilities;
using DiegoG.Utilities.Collections;
using DiegoG.Utilities.IO;
using NeoSmart.AsyncLock;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Models.DataKeeper
{
    public class UserDataTracker
    {
        private readonly AsyncLock sync = new();
        
        [Key]
        public Guid Key { get; set; }

        #region Transfer
        
        public double DailyTransferUploadQuota { get; set; }
        public double DailyTransferDownloadQuota { get; set; }

        public TransferReport GetTransferQuota()
            => new(DailyTransferUploadQuota, DailyTransferDownloadQuota);

        public byte[] TrackersList { get; set; } = Array.Empty<byte>();

        [NotMapped]
        public List<DailyUsageTracker> Trackers { get;  set; } = new();

        [NotMapped]
        public bool TrackersLoaded { get; private set; } = false;

        public async Task ClearOutdatedTransferTrackers()
        {
            DailyUsageTracker tr;
            using (await sync.LockAsync())
            {
                if (TrackersLoaded is false)
                    await LoadTrackers();

                for (int i = 0; i < Trackers.Count; i++)
                    if ((tr = Trackers[i]).Created + tr.Expiration < DateTime.Now)
                        Trackers.RemoveAt(i--);
            }
        }

        public async Task AddTracker(DailyUsageTracker tracker)
        {
            using (await sync.LockAsync())
            {
                if (TrackersLoaded is false)
                    await LoadTrackers();
                Trackers.Add(tracker);
            }
        }

        public async Task LoadTrackers()
        {
            try
            {
                Trackers = new(await Serialization.Deserialize<DailyUsageTracker[]>.MsgPkAsync(TrackersList));
            }
            catch
            {
                Trackers = new List<DailyUsageTracker>();
            }

            TrackersLoaded = true;
        }

        public async Task SaveTrackers()
            => TrackersList = await Serialization.Serialize.MsgPkAsync(Trackers.ToArray());

        public async Task<TransferReport> GetTotalTransferUsage()
        {
            using (await sync.LockAsync())
            {
                if (TrackersLoaded is false)
                    await LoadTrackers();
                return new(Trackers.Aggregate<DailyUsageTracker, (double up, double down)>((0, 0), (acc, val) => (acc.up + val.Upload, acc.down + val.Download)));
            }
        }

        public async Task<(bool IsOverDownload, bool IsOverUpload, double OverDownloadQuota, double OverUploadQuota)> GetIfOverTransferQuota()
        {
            var total = await GetTotalTransferUsage();
            double odq = total.Download - DailyTransferDownloadQuota;
            double ouq = total.Upload - DailyTransferUploadQuota;
            return (odq > 0, ouq > 0, odq, ouq);
        }

        #endregion

        #region Storage

        /// <summary>
        /// In Bytes
        /// </summary>
        public double StorageQuota { get; set; }

        /// <summary>
        /// In Bytes
        /// </summary>
        public double StorageUsage { get; set; }

        [NotMapped]
        public bool IsOverStorageQuota => StorageUsage >= StorageQuota;

        public double WouldSurpassStorageQuota(double data) => 0d.Min(StorageUsage + data - StorageQuota);

        #endregion

        public UserDataTracker() { }

        public static UserDataTracker Empty(Guid key) => new()
        {
            Key = key,
            DailyTransferDownloadQuota = 0,
            DailyTransferUploadQuota = 0,
            StorageQuota = 0,
            StorageUsage = 0
        };
    }
}
