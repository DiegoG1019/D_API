using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Enums
{
    public static class AuthorizationRoles
    {
        public const string Root = "root";
        public const string AppDataHost = "root,adh";
    }

    public static class UserAccessRoles
    {
        public const string Root = "root";
        public const string AppDataHost = "adh";
    }
}
