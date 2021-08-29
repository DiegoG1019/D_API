using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace D_API.Types
{
    public interface IAppDataAccessKeeper
    {
        public Task NewFile(ClaimsPrincipal id, string file);
        public Task<bool> CheckAccess(ClaimsPrincipal id, string file);
    }
}
