using D_API.Dependencies.Interfaces;
using D_API.Enums;
using D_API.Exceptions;
using D_API.Types.DataKeeper;
using D_API.Types.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace D_API.Controllers
{
    [Authorize(Roles = AuthorizationRoles.AppDataHost)]
    [ApiController]
    [Route("api/v1/data")]
    public class AppDataHost : D_APIController
    {
        private static readonly object DbSeedLock = new();
        private static bool IsDbSeeded = false;

        private readonly IAppDataKeeper Data;

        public AppDataHost(IAppDataKeeper keeper)
        {
            Data = keeper;
            if (IsDbSeeded is false)
                lock (DbSeedLock)
                    if (IsDbSeeded is false)
                    {
                        Data.EnsureRoot();
                        IsDbSeeded = true;
                    }
        }

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
                return Forbidden(new BadUserKey(key, error));
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(new BadDataKey(datakey, error));
            
            var op = await Data.Download(key, datakey);
            return op.Result switch
            {
                DataOpResult.DataDoesNotExist => NotFound(new DataDownloadFailure(datakey, "Could not find any matching data")),
                DataOpResult.DataInaccessible => Unauthorized(new DataDownloadFailure(datakey, "This user does not have access this data")),
                DataOpResult.OverTransferQuota => Forbidden(new DataQuotaExceeded(op.SecondValue, "download")),
                DataOpResult.Success => Ok(new DataDownloadSuccess(datakey, op.FirstValue)),
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
                return Forbidden(new BadUserKey(key, error));
            if ((error = VerifyDataKey(datakey) ?? VerifyUploadRequest(upRequest)) is not null)
                return BadRequest(new BadDataKey(datakey, error));

            var op = await Data.Upload(key, datakey, upRequest.Data!, upRequest.Overwrite);
            return op.Result switch
            {
                DataOpResult.DataInaccessible => NotFound(new DataUploadFailure(datakey, "This user does have access to this data, or it does not exist")),
                DataOpResult.OverStorageQuota => Forbidden(new DataQuotaExceeded(op.FirstValue, "storage")),
                DataOpResult.NoOverwrite => Forbidden(new DataUploadFailure(datakey, "This data already exists, and overwrite parameter is not set to true")),
                DataOpResult.OverTransferQuota => Forbidden(new DataQuotaExceeded(op.FirstValue, "upload")),
                DataOpResult.Success => Ok(new DataUploadSuccess(datakey, op.SecondValue)),
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
                return Forbidden(new BadUserKey(key, error));
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(new BadDataKey(datakey, error));

            return Ok(new AccessCheck(await Data.CheckExists(key, datakey)));
        }

        [HttpGet("transferreport")]
        public async Task<IActionResult> GetTransferReport()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbidden(new BadUserKey(key, error));

            var (tu, tq, su, sq) = await Data.GetFullTransferReport(key);

            return Ok(new TransferQuotaStatus(tu, tq, su, sq));
        }
    }
}
