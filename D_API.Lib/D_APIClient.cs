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

        public D_APIClient(Uri address, string key, D_APIClientConfig? config = null)
        {
            Http = new HttpClient() { BaseAddress = address };
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
            Config = config ?? new D_APIClientConfig();
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
        }
        public D_APIClient(HttpClient client, string? address = null, string? key = null) : this(client, address != null ? new Uri(address) : null, key) { }
    }
}