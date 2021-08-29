using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace D_API
{
    public static class Roles
    {
        public const string Root = "root";
        public const string Moderator = "mod,admin,root";
        public const string Administrator = "admin,root";
    }
    public static class Helper
    {
        public static async Task CheckAuthValidity(this ClaimsPrincipal user)
        {
            if (user.Identity?.Name is null)
                await Task.Run(() =>
                {
                    string d = $"An authorized user cannot have a null Identity or Name. Claims: \n*> {string.Join("\n*>", user.Claims.Select(x => $"Type: {x.Type}, Value: {x.Value}, Issuer: {x.Issuer}"))}";
                    Log.Error(d);
                    throw new InvalidOperationException(d);
                });
        }
    }
}