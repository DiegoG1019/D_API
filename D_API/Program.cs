using DiegoG.Utilities.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace D_API
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Settings<APISettings>.Initialize(Directories.Configuration, "apisettings.cfg");

            {
                List<string> errors = new();
                if (Settings<APISettings>.Current.APIKey is null)
                    errors.Add($"Cannot start without a valid Telegram Bot API Key. Please configure one in {Path.GetFullPath(Directories.Configuration)}");

                if (!Settings<APISettings>.Current.AllowedUsers.Any())
                    errors.Add($"Cannot start without at least one allowed user. Please configure one in {Path.GetFullPath(Directories.Configuration)}");

                if (Settings<APISettings>.Current.EventChannelId is 0)
                    errors.Add($"Cannot start without a valid Event Telegram Channel Id. Please configure one in {Path.GetFullPath(Directories.Configuration)}");

                if (errors.Any())
                    throw new InvalidOperationException($"Found one or more errors when initializing the API:\n*> {string.Join("\n*> ", errors)}");
            }

            Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Verbose()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .WriteTo.File("logs/.log", rollingInterval: RollingInterval.Hour, restrictedToMinimumLevel: LogEventLevel.Verbose)
#if DEBUG
                   .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
#else
                   .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
#endif
                   .CreateLogger();

            foreach (var x in typeof(Directories).GetFields(BindingFlags.Static | BindingFlags.Public))
                Log.Information($"{x.Name} directory @ {x.GetValue(null)}");
                
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
