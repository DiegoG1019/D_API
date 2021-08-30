using D_API.Lib.Exceptions;
using D_API.Lib.Types;
using Nito.AsyncEx;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace D_API.Lib
{
    public class D_APIClient
    {
        protected readonly AsyncLock Lock = new AsyncLock();

        protected readonly HttpClient Http;
        protected readonly D_APIClientConfig Config;
        protected readonly IRequestQueue RequestQueue;

        public D_APIClient(Uri address, string key, D_APIClientConfig? config = null)
        {
            Http = new HttpClient() { BaseAddress = address };
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            Config = config ?? new D_APIClientConfig();

            RequestQueue = Config.AutoQueue ? (IRequestQueue)new D_APIRequestQueue() : new D_APINoRequestQueue();
        }
        public D_APIClient(string address, string key) : this(new Uri(address), key) { }

        public D_APIClient(HttpClient client, Uri? address = null, string? key = null, D_APIClientConfig? config = null)
        {
            Http = client;
            if (address != null)
                Http.BaseAddress = address;
            if (key != null)
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            Config = config ?? new D_APIClientConfig();

            RequestQueue = Config.AutoQueue ? (IRequestQueue)new D_APIRequestQueue() : new D_APINoRequestQueue();
        }
        public D_APIClient(HttpClient client, string? address = null, string? key = null) : this(client, address != null ? new Uri(address) : null, key) { }

        private Role? RoleField;
        public Task<Role> GetRole() => RequestQueue.NewRequest(async () =>
        {
            using (await Lock.LockAsync())
            {
                if (RoleField is null)
                    RoleField = await Http.GetFromJsonAsync<Role>("api/test/proberole");
                return (Role)RoleField;
            }
        });

        public Task<bool> ProbeAuthRoot() => Probe_("probeauthroot");
        public Task<bool> ProbeAuthAdmin() => Probe_("probeauthadmin");
        public Task<bool> ProbeAuthMod() => Probe_("probeauthmod");
        public Task<bool> ProbeAuth() => Probe_("probeauth");
        public Task<bool> Probe() => Probe_("probe");
        private Task<bool> Probe_(string endpoint) => RequestQueue.NewRequest(async () =>
        {
            try
            {
                return (await Http.GetAsync($"api/test/{endpoint}")).IsSuccessStatusCode;
            }
            catch (TimeoutException)
            {
                return false;
            }
        });

        public Task<T> GetAppData<T>(string appname) => RequestQueue.NewRequest(async () =>
        {
            var r = await Http.GetAsync($"api/v1/appdatahost/config/{appname}");
            if (r.StatusCode is HttpStatusCode.OK)
                return (await r.Content.ReadFromJsonAsync<T>())!;
            if (r.StatusCode is HttpStatusCode.Forbidden)
                throw new D_APIException($"This client is not logged in as the owner of {appname}");
            if (r.StatusCode is HttpStatusCode.NotFound)
                throw new D_APIException($"There is no data under {appname}");
            throw new D_APIException("Unknown");
        });

        public Task PostAppData<T>(string appname, T data, bool overwrite = false) 
            => RequestQueue.NewRequest(() => Http.PostAsJsonAsync($"api/v1/appdatahost/config/{appname}/{overwrite}", data));
    }
}