using D_API.Dependencies.Interfaces;
using D_API.Models.Auth;
using D_API.Types.Auth;
using System.Collections.Generic;

namespace D_API.Dependencies.Abstract
{
    public abstract class AbstractAuthCredentialsVerifier : IAuthCredentialsVerifier
    {
        protected readonly string HashKey;

        protected AbstractAuthCredentialsVerifier(string hashKey) => HashKey = hashKey;

        public abstract Task<Client?> FindClient(Guid key);
        public abstract Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials);

        protected virtual async Task<CredentialVerificationResults> VerifyClientCredentials
            (ClientValidCredentials credentials, Client? client, bool failImmediately = false)
        {
            if (failImmediately || client is null)
                return new(CredentialVerificationResult.Forbidden, null);

            var status = client.CurrentStatus;

            if (status is Client.Status.Inactive)
                return new(CredentialVerificationResult.Unauthorized, client, "This Key is inactive");

            if (status is Client.Status.Revoked)
                return new(CredentialVerificationResult.Revoked, client);

            if (status is Client.Status.Active)
            {
                if (credentials.Identifier != client.Identifier)
                    return new(CredentialVerificationResult.Unauthorized, client, "Credentials Mismatch");
                if (await Helper.GetHashAsync(credentials.Identifier, HashKey) != client.Secret)
                    return new(CredentialVerificationResult.Unauthorized, client, "Credentials Mismatch");
                return new(CredentialVerificationResult.Authorized, client);
            }

            return new(CredentialVerificationResult.Forbidden, null);
        }
    }
}
