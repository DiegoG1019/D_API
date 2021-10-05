using D_API.Lib.Exceptions;
using D_API.Lib.Models.Data;
using D_API.Lib.Models.Responses;
using System.Data;
using System.Threading.Tasks;

namespace D_API.Lib.Components
{
    public class UserData : Component
    {
        internal UserData(D_APIClient client) : base(client) { }

        public Task<byte[]?> Download(string datakey)
            => RequestQueue.NewRequest(async () =>
            {
                var x = await Client.GetFromAPIAsync($"api/v1/data/download/{datakey}");
                return x.APIResponse.IsError ? throw APIException.GetException(x) : x.APIResponse.As<DataDownloadSuccessResponse>().Data;
            }, Internal.Endpoint.Data);

        public Task<DataUploadResults> Upload(string datakey, byte[]? data, bool overwrite)
            => RequestQueue.NewRequest(async () =>
            {
                var x = await Client.PostToAPIAsync($"api/v1/data/upload/{datakey}", new { data, overwrite });
                return x.APIResponse.IsError ? throw APIException.GetException(x) : new DataUploadResults(true, x.APIResponse.As<DataUploadSuccessResponse>());
            }, Internal.Endpoint.Data);

        public Task<bool> CheckAccess(string datakey)
            => RequestQueue.NewRequest(async () =>
            {
                var x = await Client.GetFromAPIAsync($"api/v1/data/access/{datakey}");
                return x.APIResponse.IsError ? throw APIException.GetException(x) : x.APIResponse.As<AccessCheckResponse>().IsAccessible;
            }, Internal.Endpoint.Data);

        public Task<TransferReportResults> GetTransferReport()
            => RequestQueue.NewRequest(async () =>
{
                var x = await Client.GetFromAPIAsync($"api/v1/data/transferreport");
                return x.APIResponse.IsError ? throw APIException.GetException(x) : new TransferReportResults(x.APIResponse.As<TransferQuotaStatusResponse>());
            }, Internal.Endpoint.Data);
    }
}
