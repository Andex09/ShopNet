using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ShopNet.Api.IntegrationTests;

/// <summary>Hand-made JWTs, to check that the API rejects anything it did not issue itself.</summary>
internal static class TestTokens
{
    public static string Create(
        string issuer = ApiFactory.Issuer,
        string audience = ApiFactory.Audience,
        string signingKey = ApiFactory.SigningKey,
        TimeSpan? expiresIn = null,
        string role = "Admin")
    {
        var now = DateTime.UtcNow;
        var expires = now + (expiresIn ?? TimeSpan.FromMinutes(10));

        return new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = issuer,
            Audience = audience,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = "1",
                [JwtRegisteredClaimNames.UniqueName] = ApiFactory.AdminUserName,
                ["role"] = role,
            },
            IssuedAt = expires < now ? expires.AddMinutes(-10) : now,
            NotBefore = expires < now ? expires.AddMinutes(-10) : now,
            Expires = expires,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256),
        });
    }
}
