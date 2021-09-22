using D_API.Types.Auth;
using D_API.Types.Auth.Models;

namespace D_API.Interfaces;

public interface IAuthCredentialsVerificationProvider
{
    public Task<CredentialVerificationResults> Verify(ClientValidCredentials credentials);

    public Task<Client?> FindClient(Guid key);
}
