using D_API.Models.Auth;

namespace D_API.Types.Auth
{
    public sealed record CredentialVerificationResults(CredentialVerificationResult Result, User? User, string? Message = null) { }

    public sealed record UserCreationResults(UserCreationResult? Result, UserValidCredentials? User) { }

    public enum UserCreationResult
    {
        Accepted,
        AlreadyExists,
        Denied
    }

    public enum CredentialVerificationResult
    {
        Forbidden,
        Revoked,
        Unauthorized,
        Authorized
    }
}
