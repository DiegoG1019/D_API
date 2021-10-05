using DiegoG.Utilities.IO;
using MessagePack;
using System;

namespace D_API.Models.DataKeeper
{
    [MessagePackObject]
    public class DailyUsageTracker
    {
        [Key(0)] public DateTime Created { get; set; }
        [Key(1)] public TimeSpan Expiration { get; set; }
        [Key(2)] public double Upload { get; set; }
        [Key(3)] public double Download { get; set; }

        public DailyUsageTracker() { }

        public DailyUsageTracker(double upload, double download, DateTime created, TimeSpan expiration)
        {
            Created = created;
            Expiration = expiration;
            Upload = upload;
            Download = download;
        }

        public DailyUsageTracker(double upload, double download) : this(upload, download, DateTime.Now, TimeSpan.FromDays(0.999)) { }
    }
}
