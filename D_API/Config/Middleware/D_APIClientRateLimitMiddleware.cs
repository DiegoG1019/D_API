using AspNetCoreRateLimit;
using D_API.Types.Responses;
using DiegoG.Utilities.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace D_API.Config.Middleware
{
    public class D_APIClientRateLimitMiddleware : ClientRateLimitMiddleware
    {
        public D_APIClientRateLimitMiddleware
            (RequestDelegate next,
             IProcessingStrategy processingStrategy,
             IOptions<ClientRateLimitOptions> options,
             IRateLimitCounterStore counterStore,
             IClientPolicyStore policyStore,
             IRateLimitConfiguration config,
             ILogger<ClientRateLimitMiddleware> logger) : base(next, processingStrategy, options, counterStore, policyStore, config, logger) { }

        public override async Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
            httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsync(await Serialization.Serialize.JsonAsync(new TooManyRequests(rule.Limit, rule.PeriodTimespan ?? TimeSpan.Zero, retryAfter)));
        }
    }
}
