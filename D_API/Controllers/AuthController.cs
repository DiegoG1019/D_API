using D_API.Dependencies.Interfaces;
using D_API.Exceptions;
using D_API.Models.Auth;
using D_API.Types.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using D_API.Types.Responses;

namespace D_API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : D_APIController
    {
        const string InSession = "session";
        const string Requester = "requester";
        const string AppendRequester = ",requester";

        private static readonly object DbSeedLock = new();
        private static bool IsDbSeeded = false;

        private readonly IAuthCredentialsProvider Auth;
        private readonly IJwtProvider Jwt;

        public AuthController(IAuthCredentialsProvider auth, IJwtProvider jwt)
        {
            Auth = auth;
            Jwt = jwt;

            if (IsDbSeeded is false)
                lock (DbSeedLock)
                    if (IsDbSeeded is false)
                    {
                        Auth.EnsureRoot();
                        IsDbSeeded = true;
                    }
        }

        [HttpGet("status")]
        public IActionResult VerifyAuth()
            => User.Identity?.IsAuthenticated is false ?
                    Unauthorized(new AuthStatus(false, false)) :
                    User.IsInRole(Requester) ?
                        Ok(new AuthStatus(true, true)) :
                        BadRequest(new AuthStatus(true, false));

        [Authorize(Roles = InSession)]
        [HttpGet("renew")]
        public async Task<IActionResult> RenewRequestToken()
        {
            if (User.GetUserKey(out var key, out string? error) is false) 
                return Forbidden(new BadUserKey(key, error));

            var user = await Auth.FindUser(key);
            return user?.CurrentStatus switch
            {
                null => Unauthorized(new RenewSessionFailure("A User by the given key could not be found")),
                Models.Auth.User.Status.Revoked => Unauthorized(new RenewSessionFailure("This user has had their credentials revoked")),
                Models.Auth.User.Status.Inactive => Unauthorized(new RenewSessionFailure("This user is currently inactive")),
                Models.Auth.User.Status.Active => Ok(new RenewSessionSuccess(await Task.Run(() => (Jwt.GenerateToken(user.Identifier, key, TimeSpan.FromSeconds(30), user.Roles + AppendRequester)!)))),
                _ => Error(await Report.WriteControllerReport(
                    new(DateTime.Now, new InvalidOperationException("The state of the user cannot be verified"),
                this, 
                new KeyValuePair<string, object>[]
                {
                    new ("User", user)
                }), "Authentication"), useExceptionMessage: true)
            };
        }

        [AllowAnonymous]
        [HttpPost("newsession")]
        public async Task<IActionResult> RequestToken([FromBody]UserRequestCredentials creds)
        {
            UserValidCredentials validCredentials;
            {
                List<string> errors = new(4);
                string? error;

                if (!VerifyIdentifier(creds.Identifier, out error))
                    errors.Add(error);

                if (!VerifyGuid(creds.UserKey, "Key", out error, out Guid key))
                    errors.Add(error);

                if (!VerifySecret(creds.Secret, out error))
                    errors.Add(error);

                if (errors.Any())
                    return BadRequest(new NewSessionBadRequest(errors));

                validCredentials = new(key, creds.Secret!, creds.Identifier!);
            }
        
            var r = await Auth.Verify(validCredentials);
            var res = r.Result;

            if (res is CredentialVerificationResult.NotRecognized)
            {
                Log.Information($"User {creds.Identifier} ({creds.UserKey}) tried to auth, but their credentials were not recognized");
                return Forbidden(new NewSessionFailure(res, "The user's credentials were verified, but are not recognized"));
            }

            if (res is CredentialVerificationResult.Revoked)
            {
                Log.Information($"User {r.User!} tried to auth with revoked credentials. Not Authorized");
                return Unauthorized(new NewSessionFailure(res, "The user's credentials were verified, but they have been revoked"));
            }

            if (res is CredentialVerificationResult.Refused)
            {
                Log.Information($"User {r.User!} was not Authorized");
                return Unauthorized(new NewSessionFailure(res, "The user's credentials were verified, but you have not been authorized"));
            }

            if (res is CredentialVerificationResult.Authorized) 
            {
                Log.Information($"User {r.User!} was succesfully authorized");
                return Ok(new NewSessionSuccess(Jwt.GenerateToken(r.User!.Identifier, r.User.Key, TimeSpan.FromHours(1), InSession)!));
            }

            return Error(await Report.WriteControllerReport(
                new(DateTime.Now, new InvalidOperationException("Unable to process credentials"), this, new KeyValuePair<string, object>[]
                {
                    new ("CredentialVerificationResult", r.Result.ToString()),
                    new ("User", r?.User ?? (object)"null"),
                    new ("SubmittedCredentials", creds)
                }), "Authentication"), useExceptionMessage: true);
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            if (User.Identity?.IsAuthenticated is not true)
                return Unauthorized(new RoleReport("anonymous"));

            User? cl;
            if (!User.GetUserKey(out var key, out string? error) || (cl = await Auth.FindUser(key)) is null)
                return Forbidden(new BadUserKey(key, error));

            return Ok(new RoleReport(cl.Roles.Split(',')));
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
}
