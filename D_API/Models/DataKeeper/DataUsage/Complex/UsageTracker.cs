using DiegoG.Utilities.Measures;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace D_API.Models.DataKeeper.DataUsage.Complex
{
    [ComplexType]
    public class UsageTracker
    {
        public UsageTracker(DataMeasure download, DataMeasure upload)
        {
            Upload = upload;
            Download = download;
        }

        public UsageTracker() : this(new DataMeasure(), new DataMeasure()) { }
        public UsageTracker(double download, double upload) : this(new DataMeasure(download), new DataMeasure(upload)) { }
        public UsageTracker(double download, Data.DataPrefix prefixDownload, double upload, Data.DataPrefix prefixUpload)
            : this(new DataMeasure(download, prefixDownload), new DataMeasure(upload, prefixUpload)) { }

        public DataMeasure Upload { get; set; }

        public DataMeasure Download { get; set; }
    }
}
