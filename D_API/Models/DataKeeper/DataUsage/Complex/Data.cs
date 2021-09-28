using DiegoG.Utilities.Measures;
using System.ComponentModel.DataAnnotations.Schema;

namespace D_API.Models.DataKeeper.DataUsage.Complex
{
    [ComplexType]
    public class DataMeasure : Data
    {
        public DataMeasure() : base() { }
        public DataMeasure(double value, DataPrefix prefix = DataPrefix.n) : base(value, prefix) { }

        public static DataMeasure operator +(DataMeasure a, DataMeasure b) => (DataMeasure)a.Add(b);
        public static DataMeasure operator +(DataMeasure a, double b) => (DataMeasure)a.Add(b);

        public static DataMeasure operator -(DataMeasure a, DataMeasure b) => (DataMeasure)a.Sub(b);
        public static DataMeasure operator -(DataMeasure a, double b) => (DataMeasure)a.Sub(b);
    }
}
