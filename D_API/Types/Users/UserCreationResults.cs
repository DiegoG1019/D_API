using D_API.Enums;
using D_API.Types.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace D_API.Types.Users
{
    public record UserCreationResults(UserCreationResult? Result, UserValidCredentials? Credentials, string? ReasonForDenial = null);
}
