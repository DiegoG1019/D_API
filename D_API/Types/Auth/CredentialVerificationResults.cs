using D_API.Models.Auth;

namespace D_API.Types.Auth;

public sealed record CredentialVerificationResults(CredentialVerificationResult Result, Client? Client, string? Message = null) { }

public enum CredentialVerificationResult
{
    Forbidden,
    Revoked,
    Unauthorized,
    Authorized
}