using D_API.Dependencies.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace D_API.Dependencies.Implementations;

public class JwtAuth : IJwtProvider
{
    private readonly SymmetricSecurityKey TokenKey;
    private readonly string? Audience;
    private readonly string? Issuer;
    public JwtAuth(SymmetricSecurityKey key, string? audience, string? issuer)
    {
        Audience = audience;
        Issuer = issuer;
        TokenKey = key;
    }

    public string? GenerateToken(string names, Guid key, TimeSpan duration, string roles)
        => GenerateToken(names, key, duration, roles.Split(','));

    public string? GenerateToken(string names, Guid key, TimeSpan duration, string[] roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new Claim[3 + roles.Length];
        claims[^3] = new Claim(ClaimTypes.NameIdentifier, key.ToString());
        claims[^2] = new Claim(ClaimTypes.Name, names);
        claims[^1] = new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
        for (int i = 0; i < roles.Length; i++)
            claims[i] = new Claim(ClaimTypes.Role, roles[i]);

        var now = DateTime.UtcNow;
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            NotBefore = now,
            IssuedAt = now,
            Expires = now.Add(duration),
            Subject = new ClaimsIdentity(claims, "serverAuth"),
            Audience = Audience,
            Issuer = Issuer,
            SigningCredentials = new SigningCredentials(TokenKey, SecurityAlgorithms.HmacSha256Signature)
        };

        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }
}
