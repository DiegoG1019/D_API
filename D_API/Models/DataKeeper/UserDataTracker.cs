using D_API.Models.DataKeeper.DataUsage.Complex;
using DiegoG.Utilities.Collections;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace D_API.Models.DataKeeper
{
    public class UserDataTracker
    {
        [Key]
        public Guid Key { get; set; }

        public UsageTracker DailyTransferQuota { get; set; } = new();

        public DailyUsageTracker DailyTransferUsage { get; set; } = new();

        public DataMeasure StorageQuota { get; set; } = new();

        public DataMeasure StorageUsage { get; set; } = new();

        [NotMapped]
        public bool IsOverStorageQuota => StorageUsage >= StorageQuota;
        public double WouldSurpassStorageQuota(double data) => WouldSurpassStorageQuota(new DataMeasure(data));
        public double WouldSurpassStorageQuota(DataMeasure data)
        {
            var r = StorageUsage + data - StorageQuota;
            return r > DiegoG.Utilities.Measures.Data.Zero ? r.TotalBytes : 0;
        }

        public async Task<(bool IsOverDownload, bool IsOverUpload, double OverDownloadQuota, double OverUploadQuota)> GetIfOverTransferQuota()
        {
            var total = await DailyTransferUsage.GetTotal();
            double odq = total.Download.TotalBytes - DailyTransferQuota.Download.TotalBytes;
            double ouq = total.Upload.TotalBytes - DailyTransferQuota.Upload.TotalBytes;
            return (odq > 0, ouq > 0, odq, ouq);
        }
    }
}
