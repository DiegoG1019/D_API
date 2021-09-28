using D_API.Dependencies.Interfaces;
using D_API.Models.Auth;
using D_API.Types.Auth;
using System.Collections.Generic;
using DiegoG.TelegramBot;
using DiegoG.Utilities.Settings;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using System;
using DiegoG.Utilities.IO;

namespace D_API.Dependencies.Abstract
{
    public abstract class AbstractAuthCredentialsProvider : IAuthCredentialsProvider
    {
        protected readonly string HashKey;

        protected AbstractAuthCredentialsProvider(string hashKey) => HashKey = hashKey;

        public abstract Task<User?> FindUser(Guid key);
        public abstract Task<CredentialVerificationResults> Verify(UserValidCredentials credentials);

        protected virtual async Task<CredentialVerificationResults> VerifyUserCredentials
            (UserValidCredentials credentials, User? user, bool failImmediately = false)
        {
            if (failImmediately || user is null)
                return new(CredentialVerificationResult.Forbidden, null);

            var status = user.CurrentStatus;

            if (status is User.Status.Inactive)
                return new(CredentialVerificationResult.Unauthorized, user, "This Key is inactive");

            if (status is User.Status.Revoked)
                return new(CredentialVerificationResult.Revoked, user);

            if (status is User.Status.Active)
            {
                if (credentials.Identifier != user.Identifier)
                    return new(CredentialVerificationResult.Unauthorized, user, "Credentials Mismatch");
                if (await Helper.GetHashAsync(credentials.Identifier, HashKey) != user.Secret)
                    return new(CredentialVerificationResult.Unauthorized, user, "Credentials Mismatch");
                return new(CredentialVerificationResult.Authorized, user);
            }

            return new(CredentialVerificationResult.Forbidden, null);
        }
    }
}
