using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Authentication
{
    public class JwtAuth : IAuth
    {
        private readonly string Key;
        public JwtAuth(string key)
        {
            Key = key;
        }

        public string? Authenticate(string name, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenKey = Encoding.ASCII.GetBytes(Key);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, name),
                        new Claim(ClaimTypes.Role, role)
                    }),
                Expires = null,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature)
            };

            Log.Warning($"Creating new Json Web Token: {name}, {role}");

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }
        public string? Authenticate() => null;
    }
}
