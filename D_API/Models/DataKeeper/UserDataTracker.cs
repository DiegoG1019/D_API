using D_API.Types.DataKeeper;
using DiegoG.Utilities;
using DiegoG.Utilities.Collections;
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

        public List<DailyUsageTracker> Trackers { get; set; } = new();

        public async Task ClearOutdatedTransferTrackers()
        {
            DailyUsageTracker tr;
            using (await sync.LockAsync())
                for (int i = 0; i < Trackers.Count; i++)
                    if ((tr = Trackers[i]).Created + tr.Expiration < DateTime.Now)
                        Trackers.RemoveAt(i--);
        }

        public async Task AddTracker(DailyUsageTracker tracker)
        {
            using (await sync.LockAsync())
                Trackers.Add(tracker);
        }

        public async Task<TransferReport> GetTotalTransferUsage()
        {
            using (await sync.LockAsync())
                return new(Trackers.Aggregate<DailyUsageTracker, (double up, double down)>((0, 0), (acc, val) => (acc.up + val.Upload, acc.down + val.Download)));
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

        public UserDataTracker() { }// => Trackers = Task.Run(() => SerializedTrackers.Select(x => DailyUsageTracker.Deserialize(x)).ToList());
    }
}
