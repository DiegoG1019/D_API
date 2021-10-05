using D_API.Lib.Models.Responses;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib.Components
{
    public class Auth : Component
    {
        internal Auth(D_APIClient client) : base(client) { }

        public Task<AuthStatusResponse> Status() 
            => RequestQueue.NewRequest(async () => (await Client.GetFromAPIAsync("api/v1/auth/status")).APIResponse.As<AuthStatusResponse>(), Internal.Endpoint.Auth);
    }
}
