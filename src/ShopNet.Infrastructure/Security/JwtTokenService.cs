using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Users;

namespace ShopNet.Infrastructure.Security;

internal sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenService
{
    private readonly JsonWebTokenHandler _handler = new();

    public string CreateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var settings = options.Value;
        var now = timeProvider.GetUtcNow().UtcDateTime;

        var descriptor = new SecurityTokenDescriptor
        {
            // Short JWT claim names; the API maps them back to ClaimTypes.NameIdentifier/Name/Role.
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(CultureInfo.InvariantCulture),
                [JwtRegisteredClaimNames.UniqueName] = user.UserName,
                ["role"] = user.Role,
            },
            IssuedAt = now,
            NotBefore = now,
            Expires = now.AddMinutes(settings.LifetimeMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(settings.GetSigningKeyBytes()), SecurityAlgorithms.HmacSha256),
        };

        return _handler.CreateToken(descriptor);
    }
}
