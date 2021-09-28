using DiegoG.Utilities.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace D_API.Exceptions
{
    public static class Report
    {
        public record ReportData(DateTime CreatedAt, Exception Exception, object? Data = null, params KeyValuePair<string, object>[] OtherData) 
        {
            public string StackTrace { get; } = Environment.StackTrace;
        }
        public record ControllerReportData : ReportData
        {
            public ControllerReportData
                (DateTime createdAt, Exception exception, ControllerBase controller, object? data = null, params KeyValuePair<string, object>[] otherData)
                : base(
                      createdAt, 
                      exception, 
                      new 
                      { 
                          Controller = new
                          {
                              Request = new
                              {
                                  controller.Request.Protocol,
                                  controller.Request.Headers,
                                  controller.Request.Scheme,
                                  controller.Request.Host,
                                  controller.Request.Path,
                                  controller.Request.Method,
                                  controller.Request.ContentLength,
                                  controller.Request.QueryString
                              },
                              controller.User,
                              controller.RouteData,
                              controller.ModelState
                          },
                          Data = data 
                      },
                      otherData)
            { }
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

        public static Task<Exception> WriteControllerReport
            (ControllerReportData report, string category, string? subcategory = null, [CallerMemberName] string? caller = null)
            => WriteReport(report, category, subcategory, caller);

        private static async Task WriteToFile(ReportData report, string category, string? subcategory = null, string? caller = null)
        {
            string dir = Directories.InLogs("Reports", category, subcategory ?? "");
            Directory.CreateDirectory(dir);
            await Serialization.Serialize.JsonAsync(report, dir, $"{caller ?? "Unknown"}@{DateTime.UtcNow:yyyy MMMM dd(ddd) hh(HH).mm.ss tt}");
        }
    }
}
