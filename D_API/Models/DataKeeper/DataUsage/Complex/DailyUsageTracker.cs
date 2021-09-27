using NeoSmart.AsyncLock;
using System.ComponentModel.DataAnnotations.Schema;

namespace D_API.Models.DataKeeper.DataUsage.Complex;

[ComplexType]
public class DailyUsageTracker
{
    [ComplexType]
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

    public Queue<ManagedTracker> Trackers { get; init; } = new();

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
        using (await Sync.LockAsync())
            while(Trackers.Count > 0)
            {
                var tr = Trackers.Peek();
                if (tr.Created + tr.Expiration < DateTime.Now)
                    Trackers.Dequeue();
                else
                    break;
            }
    }
}
