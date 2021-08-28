using DiegoG.Utilities.IO;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        }

        public Task<bool> CheckAccess(string role, string file)
        {
            throw new NotImplementedException();
        }

        public Task NewFile(string role, string file)
        {
            throw new NotImplementedException();
        }
    }
}
