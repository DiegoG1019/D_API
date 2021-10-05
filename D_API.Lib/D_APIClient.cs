using D_API.Lib.Components;
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

        internal readonly AsyncLock Lock = new AsyncLock();

        internal readonly HttpClient Http;
        internal readonly IRequestQueue RequestQueue;
        internal readonly Credentials Credentials;

        #endregion

        #region Endpoint Handlers

        public Auth Auth { get; private set; }
        public UserData UserData { get; private set; }
        public UserManagement UserManagement { get; private set; }

        #endregion

        #region constructors

        public D_APIClient(Credentials creds, string address, bool useRequestQueue) 
            : this(creds, new Uri(address), useRequestQueue ? (IRequestQueue)new D_APIRequestQueue() : new D_APINoRequestQueue()) { }
        public D_APIClient(Credentials creds, Uri address, IRequestQueue requestQueue)
        {
            Http = new HttpClient() { BaseAddress = address };
            RequestQueue = requestQueue;
            Credentials = creds;

            Auth = new Auth(this);
            UserData = new UserData(this);
            UserManagement = new UserManagement(this);
        }

        #endregion

        #region public

        public async Task<APIResponseMessage> GetFromAPIAsync(string uri)
        {
            await RenewRequestToken();
            return await Http.GetFromAPIAsync(uri);
        }

        public async Task<APIResponseMessage> PostToAPIAsync(string uri, object data)
        {
            await RenewRequestToken();
            return await Http.PostToAPIAsync(uri, data);
        }

        public async Task<APIResponseMessage> PostToAPIAsync<T>(string uri, T data)
        {
            await RenewRequestToken();
            return await Http.PostToAPIAsync(uri, data);
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
            var r = await Http.PostToAPIAsync("api/v1/auth/newsession", Credentials);
            return r.StatusCode is HttpStatusCode.OK
                ? r.APIResponse.As<NewSessionSuccessResponse>().Token
                : throw APIException.GetException(r);
        }

        #endregion

        #region internal

        internal async Task RenewRequestToken()
        {
            if ((await Http.GetAsync("api/v1/auth/status")).StatusCode is HttpStatusCode.OK)
                return;

            await UseSessionJWT();

            while (true)
            {
                var resp = await Http.GetFromAPIAsync("api/v1/auth/renew");
#if DEBUG
#endif
                if (resp.StatusCode is HttpStatusCode.OK && resp.APIResponse is RenewSessionSuccessResponse successResponse)
                {
                    await SetRequestJWT(successResponse.Token);
                    await UseRequestJWT();
                    break;
                }
                else
                {
                    if(resp.APIResponseCode is APIResponseCode.NotInSession)
                    {
                        await SetSessionJWT(await GetSessionToken());
                        continue;
                    }
                    throw APIException.GetException(resp);
                } 
            }

            await UseRequestJWT();
        }

        #endregion
    }
}