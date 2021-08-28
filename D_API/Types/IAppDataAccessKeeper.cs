using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Types
{
    public interface IAppDataAccessKeeper
    {
        public Task NewFile(string role, string file);
        public Task<bool> CheckAccess(string role, string file);
    }
}
