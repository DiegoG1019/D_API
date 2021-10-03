using D_API.Dependencies.Interfaces;
using D_API.Enums;
using D_API.Exceptions;
using D_API.Types.DataKeeper;
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
                return Forbidden(error);
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(error);
            
            var op = await Data.Download(key, datakey);
            return op.Result switch
            {
                DataOpResult.DataDoesNotExist => NotFound($"Could not find any data matching {datakey}"),
                DataOpResult.DataInaccessible => Unauthorized($"This user does not have access to {datakey}"),
                DataOpResult.OverTransferQuota => Forbidden($"This user has exceeded their download quota by {op.SecondValue}"),
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
                return Forbidden(error);
            if ((error = VerifyDataKey(datakey) ?? VerifyUploadRequest(upRequest)) is not null)
                return BadRequest(error);

            var op = await Data.Upload(key, datakey, upRequest.Data!, upRequest.Overwrite);
            return op.Result switch
            {
                DataOpResult.DataInaccessible => Unauthorized($"This user does have access to {datakey}, or the data does not exist"),
                DataOpResult.OverStorageQuota => Forbidden($"This user has exceeded their storage quota"),
                DataOpResult.NoOverwrite => Forbidden($"This data already exists, and overwrite parameter is not set to true"),
                DataOpResult.OverTransferQuota => Forbidden($"This user has exceeded their upload quota"),
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
                return Forbidden(error);
            if ((error = VerifyDataKey(datakey)) is not null)
                return BadRequest(error);

            return Ok(await Data.CheckExists(key, datakey));
        }

        [HttpGet("transferreport")]
        public async Task<IActionResult> GetTransferReport()
        {
            if (!User.GetUserKey(out var key, out string? error))
                return Forbidden(error);
            return Ok(new
            {
                TransferQuota = await Data.GetTransferQuota(key),
                TransferUsage = await Data.GetTransferUsage(key),
                StorageQuota = await Data.GetStorageQuota(key),
                StorageUsage = await Data.GetStorageUsage(key)
            });
        }
    }
}
