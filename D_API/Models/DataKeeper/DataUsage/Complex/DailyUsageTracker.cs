using NeoSmart.AsyncLock;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace D_API.Models.DataKeeper.DataUsage.Complex
{
    public class DailyUsageTracker
    {
        [Key]
        public Guid Key { get; set; }

        public class ManagedTracker : UsageTracker
        {
            public DateTime Created { get; set; }
            public TimeSpan Expiration { get; set; }
            public ManagedTracker(DateTime created, TimeSpan expiration, double download, double upload) : base(download, upload)
            {
                Created = created;
                Expiration = expiration;
            }
            public ManagedTracker(DateTime created, TimeSpan expiration) : this(created, expiration, 0, 0) { }
        }

        private readonly AsyncLock Sync = new();

        public List<ManagedTracker> Trackers { get; init; } = new();

        public async Task<UsageTracker> GetTotal()
        {
            double downloadTotal = 0, uploadTotal = 0;
            using (await Sync.LockAsync())
                foreach(var tracker in Trackers)
                {
                    downloadTotal += tracker.Download.TotalBytes;
                    uploadTotal += tracker.Upload.TotalBytes;
                }
            return new(downloadTotal, uploadTotal);
        }

        public async Task ClearOutdatedTrackers()
        {
            ManagedTracker tr;
            using (await Sync.LockAsync())
                for(int i = 0; i < Trackers.Count; i++)
                    if ((tr = Trackers[i]).Created + tr.Expiration < DateTime.Now)
                        Trackers.RemoveAt(i--);
        }
    }
}
