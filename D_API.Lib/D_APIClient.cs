using D_API.Lib.Exceptions;
using D_API.Lib.Types;
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
        protected readonly D_APIClientConfig Config;
        protected readonly IRequestQueue RequestQueue;

        protected readonly Credentials Credentials;

        #endregion

        #region constructors

        private D_APIClient(Credentials creds, Uri address)
        {
            Http = new HttpClient() { BaseAddress = address };
            Credentials = creds;
        }

        public D_APIClient(Credentials creds, Uri address, D_APIClientConfig? config = null) 
            : this(creds, address)
        {
            Config = config ?? new D_APIClientConfig();
            RequestQueue = Config.AutoQueue ? (IRequestQueue)new D_APIRequestQueue() : new D_APINoRequestQueue();
        }

        public D_APIClient(Credentials creds, string address, D_APIClientConfig? config = null) 
            : this(creds, new Uri(address), config)
        { }

        public D_APIClient(Credentials creds, Uri address, IRequestQueue requestQueue, D_APIClientConfig? config = null) 
            : this(creds, address, config)
            => RequestQueue = requestQueue;

        public D_APIClient(Credentials creds, string address, IRequestQueue requestQueue, D_APIClientConfig? config = null) 
            : this(creds, new Uri(address), requestQueue, config) 
        { }

        #endregion

        #region methods

        #region public

        public Task<bool> ProbeAuthRoot() => Probe_("probeauthroot", true);
        public Task<bool> ProbeAuthAdmin() => Probe_("probeauthadmin", true);
        public Task<bool> ProbeAuthMod() => Probe_("probeauthmod", true);
        public Task<bool> ProbeAuth() => Probe_("probeauth", true);
        public Task<bool> Probe() => Probe_("probe", false);

        public Task<T> GetAppData<T>(string appname) => RequestQueue.NewRequest(async () =>
        {
            await RenewRequestToken();
            var r = await CheckResponse(await Http.GetAsync($"api/v1/appdatahost/config/{appname}"));

            
            
            if (r.StatusCode is HttpStatusCode.OK)
            {
                string jsonstr = await r.Content.ReadAsStringAsync();

                if (jsonstr is T resultstring)
                    return resultstring;

                try
                {
                    return JsonSerializer.Deserialize<T>(jsonstr)!;
                }
                catch (JsonException e)
                {
                    throw new D_APIInvalidDataException("The received data was invalid", e);
                }
            }
            if (r.StatusCode is HttpStatusCode.Forbidden)
                throw new D_APIUnauthorizedDataAccessException($"This client is not logged in as the owner of {appname}");
            if (r.StatusCode is HttpStatusCode.NotFound)
                throw new D_APINoDataException($"There is no data under {appname}");
            throw new D_APIException("Unknown");
        }, Endpoint.General);

        public Task PostAppData<T>(string appname, T data, bool overwrite = false) 
            => RequestQueue.NewRequest(async () => 
            {
                await RenewRequestToken();
                await CheckResponse(await Http.PostAsJsonAsync($"api/v1/appdatahost/config/{appname}/{overwrite}", data));
            }, Endpoint.General);

        private string? RoleField;
        public Task<string> GetRole() => RequestQueue.NewRequest(async () =>
        {
            await RenewRequestToken();
            using (await Lock.LockAsync())
            {
                if (RoleField is null)
                    RoleField = await HttpContentJsonExtensions.ReadFromJsonAsync<string>((await CheckResponse(await Http.GetAsync("api/test/proberole"))).Content);
                return RoleField!;
            }
        }, Endpoint.Probe);

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

        private Task<bool> Probe_(string endpoint, bool renew) => RequestQueue.NewRequest(async () =>
        {
            try
            {
                if (renew)
                    await RenewRequestToken();
                return (await CheckResponse((await Http.GetAsync($"api/test/{endpoint}")))).IsSuccessStatusCode;
            }
            catch (TimeoutException)
            {
                return false;
            }
        }, Endpoint.Probe);

        private async Task<string> GetSessionToken()
        {
            var r = await CheckResponse(await Http.SendAsync(new HttpRequestMessage()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Http.BaseAddress + "api/v1/auth/newsession"),
                Content = new StringContent(Credentials.ToJson(), Encoding.UTF8, "application/json")
            }));

            if (r.StatusCode is HttpStatusCode.OK)
                return await r.Content.ReadAsStringAsync();

            if (r.StatusCode is HttpStatusCode.Unauthorized)
                throw new D_APIUnauthorizedLoginException($"Server Responded with {r.StatusCode}: {await r.Content.ReadAsStringAsync()}");

            throw new D_APIException($"Server responded with {r.StatusCode}: {await r.Content.ReadAsStringAsync()}");
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
                    if (reason != null && reason != "" && !reason.StartsWith('{'))
                        throw new D_APIUnauthorizedLoginException(reason);
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