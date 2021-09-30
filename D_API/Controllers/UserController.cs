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

    }
}
