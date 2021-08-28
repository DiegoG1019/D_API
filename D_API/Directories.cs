using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace D_API
{
    public static class Directories
    {
        public readonly static string EnvDataDir = Environment.GetEnvironmentVariable("D_API_dir") ?? Path.Combine(Environment.CurrentDirectory, ".dat");

        public readonly static string Configuration = Path.Combine(EnvDataDir, ".config");
        public readonly static string Data = Path.Combine(EnvDataDir, ".data");
        public readonly static string Logs = Path.Combine(EnvDataDir, ".logs");
        public readonly static string AppDataHost = Path.Combine(EnvDataDir, "adh");
        
        static Directories()
        {
            foreach (var i in typeof(Directories).GetFields())
                Directory.CreateDirectory((string)i.GetValue(null)!);
        }

        public static string InConfiguration(params string[] path)
            => Path.Combine(Configuration, Path.Combine(path));

        public static string InData(params string[] path)
            => Path.Combine(Data, Path.Combine(path));

        public static string InLogs(params string[] path)
            => Path.Combine(Logs, Path.Combine(path));

        public static string InAppDataHost(params string[] path)
            => Path.Combine(AppDataHost, Path.Combine(path));
    }
}
