using D_API.Dependencies.Interfaces;
using D_API.Exceptions;
using D_API.Types.DataKeeper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Requests.Abstractions;

namespace D_API.Controllers
{
    [Authorize(Roles = Roles.AppDataHost)]
    [ApiController]
    [Route("api/v1/appdatahost")]
    public class AppDataHost : Controller
    {
        private readonly IAppDataKeeper Data;
        public AppDataHost(IAppDataKeeper keeper) => Data = keeper;

        private static string? VerifyDataKey(string datakey) 
            => datakey.Length is 0 or > 100 ? "The requested Datakey must be between 0 and 100 characters in length" : null;

        private static string? VerifyUploadRequest(UploadRequest request)
        {
            if (request.Data is null)
                return "Data cannot be null";
            return null;
        }

        [HttpGet("download/{datakey}")]
        public async Task<IActionResult> Download(string datakey)
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(error);
            
            var op = await Data.Download(key, datakey);
            return op.Result switch
            {
                DataOpResult.DataDoesNotExist => NotFound($"Could not find any data matching {datakey}"),
                DataOpResult.DataInaccessible => Unauthorized($"This client does not have access to {datakey}"),
                DataOpResult.OverTransferQuota => Forbid($"This client has exceeded their download quota by {op.SecondValue}"),
                DataOpResult.Success => Ok(op.FirstValue),
                _ => throw await Report.WriteControllerReport(new(
                    DateTime.Now,
                    new InvalidOperationException($"Expected only DataDoesNotExist, DataInaccessible, OverTransferQuota or Success, received: {op.Result}"),
                    this,
                    null,
                    new KeyValuePair<string, object>[]
                        {
                            new("DataKey", datakey),
                            new("OperationResults", op)
                        }
                    )
                    , "AppDataHost")
            };
        }

        [HttpPost("upload/{datakey}")]
        public async Task<IActionResult> Upload(string datakey, [FromBody]UploadRequest upRequest)
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            if ((error = VerifyDataKey(datakey) ?? VerifyUploadRequest(upRequest)) is not null)
                return BadRequest(error);

            var op = await Data.Upload(key, datakey, upRequest.Data!, upRequest.Overwrite);
            return op.Result switch
            {
                DataOpResult.DataInaccessible => Unauthorized($"This client does have access to {datakey}, or the data does not exist"),
                DataOpResult.OverStorageQuota => Forbid($"This client has exceeded their storage quota"),
                DataOpResult.NoOverwrite => Forbid($"This data already exists, and overwrite parameter is not set"),
                DataOpResult.OverTransferQuota => Forbid($"This client has exceeded their upload quota"),
                DataOpResult.Success => Ok(),
                _ => throw await Report.WriteControllerReport(new(
                    DateTime.Now,
                    new InvalidOperationException($"Expected only OverStorageQuota, NoOverwrite, OverTransferQuota or Success, received: {op.Result}"),
                    this,
                    null, 
                    new KeyValuePair<string, object>[]
                        {
                            new("DataKey", datakey),
                            new("UploadRequest", upRequest),
                            new("OperationResults", op)
                        }
                    )
                    , "AppDataHost")
            };
        }

        [HttpGet("access/{datakey}")]
        public async Task<IActionResult> CheckAccess(string datakey)
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(error);

            return Ok(await Data.CheckExists(key, datakey));
        }

        [HttpGet("transferquota")]
        public async Task<IActionResult> GetTransferQuota()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            return Ok(await Data.GetTransferQuota(key));
        }

        [HttpGet("transferusage")]
        public async Task<IActionResult> GetTransferUsage()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            return Ok(await Data.GetTransferUsage(key));
        }

        [HttpGet("storagequota")]
        public async Task<IActionResult> GetStorageQuota()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            return Ok(await Data.GetStorageQuota(key));
        }

        [HttpGet("storageusage")]
        public async Task<IActionResult> GetStorageUsage()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbid(error);
            return Ok(await Data.GetStorageUsage(key));
        }
    }
}
