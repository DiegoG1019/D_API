using D_API.Types.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace D_API.Config.Middleware
{
    public static class AuthorizationMiddleware
    {
        public static async Task UnauthorizedFilter(HttpContext context, Func<Task> next)
        {
            await next();
            if (context.Response is { StatusCode: (int)HttpStatusCode.Unauthorized } and { ContentLength: null or 0 })
                await context.Response.WriteAsJsonAsync(new NotInSession());
        }
    }
}
