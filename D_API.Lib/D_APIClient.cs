using D_API.Lib.Exceptions;
using D_API.Lib.Internal;
using D_API.Lib.Models;
using D_API.Lib.Models.Responses;
using Nito.AsyncEx;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace D_API.Lib
{
    public class D_APIClient
    {
        #region fields

        protected readonly AsyncLock Lock = new AsyncLock();

        protected readonly HttpClient Http;
        protected readonly IRequestQueue RequestQueue;
        protected readonly Credentials Credentials;

        #endregion

        #region constructors

        public D_APIClient(Credentials creds, Uri address, IRequestQueue requestQueue)
        {
            Http = new HttpClient() { BaseAddress = address };
            RequestQueue = requestQueue;
            Credentials = creds;
        }

        #endregion

        #region private 

        private bool sessionJwtInUse;
        private string? RequestJWT;
        private string? SessionJWT;
        private async Task SetRequestJWT(string? value)
        {
            RequestJWT = value;
            await UseRequestJWT();
        }

        private async Task SetSessionJWT(string? value)
        {
            SessionJWT = value;
            await UseSessionJWT();
        }

        private async Task UseRequestJWT()
        {
            using (await Lock.LockAsync())
            {
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", RequestJWT);
                sessionJwtInUse = false;
            }
        }

        private async Task UseSessionJWT()
        {
            using (await Lock.LockAsync())
            {
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SessionJWT);
                sessionJwtInUse = true;
            }
        }

        private async Task<string> GetSessionToken()
        {
            var r = await CheckResponse(await Http.PostAsJsonAsync("api/v1/auth/newsession", Credentials));
            return r.StatusCode is HttpStatusCode.OK
                ? await r.Content.ReadAsStringAsync()
                : throw APIException.GetException(await r.Content.ReadAsStringAsync());
        }

        private async Task RenewRequestToken()
        {
            if ((await Http.GetAsync("api/v1/auth/status")).StatusCode is HttpStatusCode.OK)
                return;

            await UseSessionJWT();

            while (true)
            {
                var resp = await CheckResponse(await Http.GetAsync("api/v1/auth/renew"));
#if DEBUG
                var response = await resp.Content.ReadAsStringAsync();
#endif
                if (resp.StatusCode is HttpStatusCode.OK)
                {
                    await SetRequestJWT(await resp.Content.ReadAsStringAsync());
                    await UseRequestJWT();
                    break;
                }
                else
                {
                    string reason = await resp.Content.ReadAsStringAsync();
                    await SetSessionJWT(await GetSessionToken());
                } 
            }

            await UseRequestJWT();
        }

        #endregion

        #endregion

        #region static methods

        #region protected

        protected static async Task<HttpResponseMessage> CheckResponse(HttpResponseMessage response)
        {
            var code = response.StatusCode;
            if (code is HttpStatusCode.TooManyRequests)
                throw new D_APITooManyRequestsException($"{response.ReasonPhrase} | {await response.Content.ReadAsStringAsync()}");
            return response;
        }

        #endregion

        #endregion
    }
}