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
