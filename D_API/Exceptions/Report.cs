using DiegoG.Utilities.IO;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace D_API.Exceptions
{
    public static class Report
    {
        public record ReportData(DateTime CreatedAt, Exception Exception, object? Data = null, params KeyValuePair<string, string>[] OtherData) 
        {
        }

        /// <summary>
        /// Writes the report and throws the reported Exception
        /// </summary>
        /// <param name="exc"></param>
        /// <returns></returns>
        public static async Task WriteReportAndThrow(ReportData report, string category, string? subcategory = null, [CallerMemberName] string? caller = null)
        {
            await WriteToFile(report, category, subcategory, caller);
            throw report.Exception;
        }

        /// <summary>
        /// Writes the report and returns the reported Exception for throwing
        /// </summary>
        /// <param name="report"></param>
        /// <param name="category"></param>
        /// <param name="subcategory"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static async Task<Exception> WriteReport(ReportData report, string category, string? subcategory = null, [CallerMemberName]string? caller = null)
        {
            await WriteToFile(report, category, subcategory, caller);
            return report.Exception;
        }

        private static async Task WriteToFile(ReportData report, string category, string? subcategory = null, string? caller = null)
        {
            string dir = Directories.InLogs("Reports", category, subcategory ?? "");
            Directory.CreateDirectory(dir);
            await Serialization.Serialize.JsonAsync(report, dir, $"{caller ?? "Unknown"}@{DateTime.UtcNow:yyyy MMMM dd(ddd) hh(HH).mm.ss tt}");
        }
    }
}
