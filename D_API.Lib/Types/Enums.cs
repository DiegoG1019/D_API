using System;
using System.Collections.Generic;
using System.Text;

namespace D_API.Lib.Types
{
    public enum Roles
    {
        Unauthenticated = -1,
        None = 0,
        Moderator = 1,
        Administrator = 2,
        Root = 3
    }
}
