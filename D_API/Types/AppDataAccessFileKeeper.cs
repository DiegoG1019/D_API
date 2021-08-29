using DiegoG.Utilities.IO;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace D_API.Types
{
    public class AppDataAccessFileKeeper : IAppDataAccessKeeper
    {
        private static readonly List<string> LoadedFiles = new();

        private readonly AsyncLock Mutex = new();
        private readonly Dictionary<string, string> AccessDict;
        private readonly string Filename;

        public AppDataAccessFileKeeper(string filename)
        {
            lock (LoadedFiles)
            {
                if (LoadedFiles.Contains(filename))
                    throw new InvalidOperationException($"AppDataAccessFileKeeper {filename} is already loaded");
                LoadedFiles.Add(filename);
            }
            Filename = filename;
            AccessDict = File.Exists(Directories.InData("AppDataAccess", Filename)) ?
                Serialization.Deserialize<Dictionary<string, string>>.Json(Directories.InData("AppDataAccess"), Filename) :
                new();
        }

        public async Task<bool> CheckAccess(ClaimsPrincipal id, string file)
        {
            using (await Mutex.LockAsync())
                return !AccessDict.ContainsKey(file) || id.IsInRole("root") || AccessDict[file] == id.Identity!.Name!;
        }

        public async Task NewFile(ClaimsPrincipal id, string file)
        {
            if (AccessDict.TryGetValue(file, out var result) && id.Identity!.Name! == result)
                return;
            using (await Mutex.LockAsync())
            {
                AccessDict[file] = id.Identity!.Name!;
                await Serialization.Serialize.JsonAsync(AccessDict, Directories.InData("AppDataAccess"), Filename);
            }
        }
    }
}
