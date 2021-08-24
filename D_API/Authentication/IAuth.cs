using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Authentication
{
    public interface IAuth
    {
        string Authenticate(string username, string password);
    }
}
