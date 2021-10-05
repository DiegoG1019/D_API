using D_API.Lib.Internal;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace D_API.Lib.Components
{
    public abstract class Component
    {
        protected readonly D_APIClient Client;
        protected readonly IRequestQueue RequestQueue;
        protected readonly HttpClient Http;

        internal Component(D_APIClient client)
        {
            Client = client;
            RequestQueue = client.RequestQueue;
            Http = client.Http;
        }
    }
}
