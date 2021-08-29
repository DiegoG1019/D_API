﻿using DiegoG.Utilities.IO;
using Nito.AsyncEx;
using Serilog;
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
            int r;
            await id.CheckAuthValidity();
            using (await Mutex.LockAsync())
                r = !AccessDict.ContainsKey(file) ? AccessDict[file] == id.Identity!.Name ? 1 : id.IsInRole("root") ? 2 : 0 : 0;
            Log.Information($"User {id.Identity!.Name} accessed {file} because they're {(r is 1 ? "the owner" : r is 2 ? "root" : "neither. ERROR!")}.");
            return r > 0;
        }

        public async Task NewFile(ClaimsPrincipal id, string file)
        {
            await id.CheckAuthValidity();
            if (AccessDict.TryGetValue(file, out var result))
                if (id.Identity!.Name! == result)
                    return;
                else if (id.IsInRole("root"))
                    return;

            Log.Information($"User {id.Identity!.Name!}");
            using (await Mutex.LockAsync())
            {
                AccessDict[file] = id.Identity!.Name!;
                await Serialization.Serialize.JsonAsync(AccessDict, Directories.InData("AppDataAccess"), Filename);
            }
        }
    }
}
