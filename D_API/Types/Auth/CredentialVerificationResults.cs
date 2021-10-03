using D_API.Models.Auth;
using System;
using System.Collections.Generic;

namespace D_API.Types.Auth
{
    public record CredentialVerificationResults(CredentialVerificationResult Result, User? User, string? Message = null) { }
    
    public enum CredentialVerificationResult
    {
        NotRecognized,
        Revoked,
        Refused,
        Authorized
    }
}
