using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace D_API.Lib
{
    public class Client
    {
        protected HttpClient Http { get; private set; }

        public Client(Uri address, string key)
        {
            Http = new HttpClient() { BaseAddress = address };
            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        }
        public Client(string address, string key) : this(new Uri(address), key) { }

        public Client(HttpClient client, Uri? address = null, string? key = null)
        {
            Http = client;
            if (address != null)
                Http.BaseAddress = address;
            if (key != null)
                Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        }
        public Client(HttpClient client, string? address = null, string? key = null) : this(client, address != null ? new Uri(address) : null, key) { }
    }
}
