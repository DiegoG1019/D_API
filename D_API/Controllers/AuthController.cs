using D_API.Exceptions;
using D_API.Interfaces;
using D_API.Types.Auth;
using D_API.Types.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace D_API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    const string InSession = "session";
    const string Requester = "requester";
    const string AppendRequester = $",{Requester}";

    private readonly IAuthCredentialsVerificationProvider Auth;
    private readonly IJwtProvider Jwt;

    public AuthController(IAuthCredentialsVerificationProvider auth, IJwtProvider jwt)
    {
        Auth = auth;
        Jwt = jwt;
    }

    [AllowAnonymous]
    [HttpGet("status")]
    public IActionResult VerifyAuth() 
        => User.Identity?.IsAuthenticated is true ? 
           User.IsInRole(Requester) ? 
           Ok() : 
           Unauthorized("The received JWT is not a request JWT, did you use a session JWT?") : 
           Unauthorized();

    [AllowAnonymous]
    [HttpGet("renew")]
    public async Task<IActionResult> RenewRequestToken()
    {
        if (User.Identity?.IsAuthenticated is false)
            return Unauthorized("This client is unauntheticated");

        Guid key;
        {
            Claim? c;
            if ((c = User.Claims.FirstOrDefault(x => x.Type is ClaimTypes.NameIdentifier)) is null)
                return Forbid("The claims of this client are invalid");
            if (!Guid.TryParse(c.Value, out key))
                return Forbid("The client's key is invalid");
        }

        var client = await Auth.FindClient(key);

        if (client is null)
            return Unauthorized("A Client by the given key could not be found");

        if (client.CurrentStatus is Client.Status.Revoked)
            return Unauthorized("This client has had their credentials revoked");

        if (client.CurrentStatus is Client.Status.Inactive)
            return Unauthorized("This client is currently inactive");

        if (client.CurrentStatus is Client.Status.Active)
            return Ok(await Task.Run(() => Jwt.GenerateToken(client.Identifier, key, TimeSpan.FromSeconds(30), client.Roles + AppendRequester)));

        throw await Report.WriteReport(new(DateTime.Now, new InvalidOperationException("The state of the client cannot be verified"), Request, new KeyValuePair<string, string>[]
            {
                new ("Client", await Serialization.Serialize.JsonAsync(client))
            }), "Authentication");
    }

    [AllowAnonymous]
    [HttpGet("newsession")]
    public async Task<IActionResult> RequestToken([FromBody]ClientRequestCredentials creds)
    {
        ClientValidCredentials validCredentials;
        {
            List<string> errors = new(4);
            string? error;

            if (!VerifyIdentifier(creds.Identifier, out error))
                errors.Add(error);

            if (!VerifyGuid(creds.Key, "Key", out error, out Guid key))
                errors.Add(error);

            if (!VerifySecret(creds.Secret, out error))
                errors.Add(error);

            if (errors.Any())
                return BadRequest(string.Join(", ", errors));

            validCredentials = new(key, creds.Secret!, creds.Identifier!);
        }
        
        var r = await Auth.Verify(validCredentials);
        var res = r.Result;

        if (res is CredentialVerificationResult.Authorized) 
        {
            Log.Information($"Client {r.Client!} was succesfully authorized");
            return Ok(Jwt.GenerateToken(r.Client!.Identifier, r.Client.Key, TimeSpan.FromHours(1), InSession));
        }

        if (res is CredentialVerificationResult.Unauthorized)
        {
            Log.Information($"Client {r.Client!} was not Authorized");
            return Unauthorized("The client credentials were verified, but you have not been authorized");
        }

        if (res is CredentialVerificationResult.Revoked)
        {
            Log.Information($"Client {r.Client!} tried to auth with revoked credentials. Not Authorized");
            return Unauthorized("The client credentials were verified, but they have been revoked");
        }

        if (res is CredentialVerificationResult.Forbidden)
        {
            Log.Information($"Client {r.Client!} tried to auth, but their credentials were not recognized");
            Forbid("The client credentials were verified, but are not recognized");
        }

        throw await Report.WriteReport(new(DateTime.Now, new InvalidOperationException("Unable to process credentials"), Request, new KeyValuePair<string, string>[]
            {
                new ("CredentialVerificationResult", r.Result.ToString()),
                new ("Client", r.Client is not null ? await Serialization.Serialize.JsonAsync(r.Client) : "null")
            }), "Authentication");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool VerifyIdentifier(string? id, [NotNullWhen(false)] out string? error)
    {
        if (id is null)
        {
            error = "Identifier must not be null";
            return false;
        }

        if (id.Length is > 256)
        {
            error = "Identifier length must be less than 256";
            return false;
        }

        if (id.Length is 0)
        {
            error = "Identifier length must be more than 0";
            return false;
        }

        error = null;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool VerifyGuid(string? guid, string name, [NotNullWhen(false)] out string? error, [NotNullWhen(true)] out Guid result)
    {
        if (guid is null)
        {
            result = default;
            error = $"{name} must not be null";
            return false;
        }

        if (guid.Length is not 36)
        {
            result = default;
            error = $"{name} length must be 36";
            return false;
        }

        if (!Guid.TryParse(guid, out result))
        {
            result = default;
            error = $"{name} is not valid";
            return false;
        }

        error = null;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool VerifySecret(string? secret, [NotNullWhen(false)] out string? error)
    {
        if (secret is null)
        {
            error = "Secret cannot be null";
            return false;
        }

        if (secret.Length is not 64)
        {
            error = "Secret's length must be 64 characters";
            return false;
        }

        error = null;
        return true;
    }
}
