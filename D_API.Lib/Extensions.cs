using D_API.Lib.Exceptions;
using D_API.Lib.Models.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Lib
{
    public static class APIResponseContentExtensions
    {
        public static async Task<APIResponseMessage> GetFromAPIAsync(this HttpClient client, string uri)
        {
            var r = await client.GetAsync(uri);
            return new APIResponseMessage(r, await r.Content.ReadAPIResponseAsync());
        }

        public static async Task<APIResponseMessage> PostToAPIAsync(this HttpClient client, string uri, object data)
        {
            var r = await client.PostAsJsonAsync(uri, data);
            return new APIResponseMessage(r, await r.Content.ReadAPIResponseAsync());
        }

        public static async Task<APIResponseMessage> PostToAPIAsync<T>(this HttpClient client, string uri, T data)
        {
            var r = await client.PostAsJsonAsync(uri, data);
            return new APIResponseMessage(r, await r.Content.ReadAPIResponseAsync());
        }

        public static async Task<APIResponse> ReadAPIResponseAsync(this HttpContent content)
            => APIResponse.GetResponse(await content.ReadAsStringAsync());

        public static async Task<T> ReadAPIResponseAsync<T>(this HttpContent content) where T : APIResponse
            => APIResponse.GetResponse<T>(await content.ReadAsStringAsync());

        public static async Task<APIException> GetAPIExceptionAsync(this HttpContent content)
            => APIException.GetException(await content.ReadAsStringAsync());
    }
}
