using D_API.Dependencies.Interfaces;
using D_API.Enums;
using D_API.Exceptions;
using D_API.Models.Auth;
using D_API.Types.Auth;
using D_API.Types.Users;
using DiegoG.Utilities.Measures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace D_API.Controllers
{
    [ApiController]
    [Route("api/v1/user")]
    public class UserController : D_APIController
    {
        private static readonly object DbSeedLock = new();
        private static bool IsDbSeeded = false;

        private readonly IAuthCredentialsProvider Auth;
        private readonly IAppDataKeeper DataKeeper;

        public UserController(IAuthCredentialsProvider auth, IAppDataKeeper keeper)
        {
            Auth = auth;
            DataKeeper = keeper;

            if (IsDbSeeded is false)
                lock (DbSeedLock)
                    if (IsDbSeeded is false)
                    {
                        Auth.EnsureRoot();
                        DataKeeper.EnsureRoot();
                        IsDbSeeded = true;
                    }
        }

        [HttpPost("create")]
        [Authorize(Roles = AuthorizationRoles.Root)]
        public async Task<IActionResult> CreateAccount([FromBody] UserCreationData newUser)
        {
            UserCreationResults? r = null;
            try
            {
                r = await Auth.Create(newUser);
                if (r.Result is UserCreationResult.AlreadyExists)
                    return Forbidden("An user with this Identifier already exists", r.ReasonForDenial);
                if (r.Result is UserCreationResult.Denied)
                    return Forbidden("Your request to create a new user has been denied", r.ReasonForDenial);
                if (r.Result is UserCreationResult.Accepted)
                {
                    if(r.ServiceData is not null)
                    {
                        HashSet<Service> Data = new();
                        foreach(var serdat in r.ServiceData)
                        {
                            if (Data.Contains(serdat.Service))
                                throw new InvalidOperationException("Cannot configure the same user service more than once");
                            Data.Add(serdat.Service);
                            if (serdat is { Service: Service.Data })
                            {
                                if(serdat.ToDataKeeperQuotaSettings() is { Success: true } and { Data: IAppDataKeeper.DataKeeperQuotaSettings dat })
                                {
                                    await DataKeeper.SetUserQuotas(r.Credentials!.Key, dat);
                                    r.ServiceConfigurationResults.Add($"Succesfully set UserQuotas to Upload:{dat.Upload}, Download:{dat.Download}, Storage:{dat.Storage}");
                                    continue;
                                }
                                r.ServiceConfigurationResults.Add($"DataKeeper Quota Settings were invalid, did not configure");
                            }
                            throw new InvalidOperationException("The given Service configuration Data does not represent any supported Service available for this user");
                        }
                    }

                    return Ok(r);
                }

                throw await Report.WriteControllerReport(
                            new(DateTime.Now, new InvalidOperationException("User Creation could not be completed"),
                                this,
                                new KeyValuePair<string, object?>[]
                                {
                                new ("UserCreationData", newUser),
                                new ("UserCreationResults", r),
                                }), "Authentication");
            }
            catch (Exception e)
            {
                throw await Report.WriteControllerReport(
                        new(DateTime.Now, e, this,
                            new KeyValuePair<string, object?>[]
                            {
                                new ("UserCreationData", newUser),
                                new ("UserCreationResults", r),
                            }), "Authentication");
            }
        }
    }
}
