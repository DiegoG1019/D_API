using D_API.Types.Auth.Models;

namespace D_API.Types.Auth;

public sealed record CredentialVerificationResults(CredentialVerificationResult Result, Client? Client) { }

public enum CredentialVerificationResult
{
    Forbidden,
    Revoked,
    Unauthorized,
    Authorized,
}