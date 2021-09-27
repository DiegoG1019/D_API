using D_API.DataContexts;
using D_API.Models.Auth;
using D_API.Types.Auth;

namespace D_API.Dependencies.Interfaces;

public interface IAuthCredentialsVerifier
{
    public Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials);

    public Task<Client?> FindClient(Guid key);
}
