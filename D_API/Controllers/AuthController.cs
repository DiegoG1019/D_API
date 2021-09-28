using D_API.Dependencies.Interfaces;
using D_API.Exceptions;
using D_API.Models.Auth;
using D_API.Types.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using DiegoG.TelegramBot;
using DiegoG.Utilities.Settings;
using Serilog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DiegoG.Utilities.IO;

namespace D_API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        const string InSession = "session";
        const string Requester = "requester";
        const string AppendRequester = ",requester";

        private readonly IAuthCredentialsProvider Auth;
        private readonly IJwtProvider Jwt;

        public AuthController(IAuthCredentialsProvider auth, IJwtProvider jwt)
        {
            Auth = auth;
            Jwt = jwt;
        }

        [HttpGet("status")]
        public IActionResult VerifyAuth()
            => User.Identity?.IsAuthenticated is false ? 
                    Unauthorized() :
                    User.IsInRole(Requester) ?
                        Ok() :
                        BadRequest("The received JWT is not a request JWT, did you use a session JWT?");

        [Authorize(Roles = InSession)]
        [HttpGet("renew")]
        public async Task<IActionResult> RenewRequestToken()
        {
            if (User.GetUserKey(out var key, out string? error) is false) 
                return Forbid(error);

            var user = await Auth.FindUser(key);
            return user?.CurrentStatus switch
            {
                null => base.Unauthorized("A User by the given key could not be found"),
                Models.Auth.User.Status.Revoked => base.Unauthorized("This user has had their credentials revoked"),
                Models.Auth.User.Status.Inactive => base.Unauthorized("This user is currently inactive"),
                Models.Auth.User.Status.Active => base.Ok(await Task.Run<string>(() => Jwt.GenerateToken(user.Identifier, key, TimeSpan.FromSeconds(30), user.Roles + AppendRequester))),
                _ => throw await Report.WriteControllerReport(
                    new(DateTime.Now, new InvalidOperationException("The state of the user cannot be verified"),
                this, 
                new KeyValuePair<string, object>[]
                {
                    new ("User", user)
                }), "Authentication")
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

            if (res is CredentialVerificationResult.Forbidden)
            {
                Log.Information($"User {creds.Identifier} ({creds.Key}) tried to auth, but their credentials were not recognized");
                return Forbid("The user's credentials were verified, but are not recognized");
            }

            if (res is CredentialVerificationResult.Revoked)
            {
                Log.Information($"User {r.User!} tried to auth with revoked credentials. Not Authorized");
                return Unauthorized("The user's credentials were verified, but they have been revoked");
            }

            if (res is CredentialVerificationResult.Unauthorized)
            {
                Log.Information($"User {r.User!} was not Authorized");
                return Unauthorized("The user's credentials were verified, but you have not been authorized");
            }

            if (res is CredentialVerificationResult.Authorized) 
            {
                Log.Information($"User {r.User!} was succesfully authorized");
                return Ok(Jwt.GenerateToken(r.User!.Identifier, r.User.Key, TimeSpan.FromHours(1), InSession));
            }

            throw await Report.WriteControllerReport(
                new(DateTime.Now, new InvalidOperationException("Unable to process credentials"), this, new KeyValuePair<string, object>[]
                {
                    new ("CredentialVerificationResult", r.Result.ToString()),
                    new ("User", r.User ?? (object)"null"),
                    new ("SubmittedCredentials", creds)
                }), "Authentication");
        }

        [AllowAnonymous]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            if (User.Identity?.IsAuthenticated is not true)
                return Ok("Unauthenticated");

            User? cl;
            if (!User.GetUserKey(out var key, out string? error) || (cl = await Auth.FindUser(key)) is null)
                return Forbid(error ?? "Could not find an user by the given key");

            return Ok(cl.Roles);
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
