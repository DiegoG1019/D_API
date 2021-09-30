using DiegoG.Utilities.IO;
using System;
using System.ComponentModel.DataAnnotations;

namespace D_API.Models.DataKeeper
{
    public class DailyUsageTracker
    {
        [Key]
        public Guid Index { get; set; }

        public DateTime Created { get; set; }
        public TimeSpan Expiration { get; set; }
        public double Upload { get; set; }
        public double Download { get; set; }

        public DailyUsageTracker(Guid index, double upload, double download, DateTime created, TimeSpan expiration)
        {
            Index = index;
            Created = created;
            Expiration = expiration;
            Upload = upload;
            Download = download;
        }

        public DailyUsageTracker(double upload, double download, DateTime created, TimeSpan expiration) : this(Guid.NewGuid(), upload, download, created, expiration) { }

        public DailyUsageTracker(double upload, double download) : this(upload, download, DateTime.Now, TimeSpan.FromDays(1)) { }
    }
}
