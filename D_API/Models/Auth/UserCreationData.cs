﻿using D_API.Types.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Models.Auth
{
    public record UserCreationData(string Identifier, string Roles, List<SerializedUserServiceData>? Data = null) { }
}
