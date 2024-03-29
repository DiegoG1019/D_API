﻿using D_API.DataContexts;
using D_API.Models.Auth;
using D_API.Types.Auth;
using DiegoG.TelegramBot;
using DiegoG.Utilities.Settings;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DiegoG.Utilities.IO;
using D_API.Types.Users;

namespace D_API.Dependencies.Interfaces
{
    public interface IAuthCredentialsProvider
    {
        public Task<CredentialVerificationResults> Verify(UserValidCredentials credentials);
        public Task<UserCreationResults> Create(UserCreationData newUser);

        public Task<User?> FindUser(Guid key);

        public bool EnsureRoot();
    }
}
