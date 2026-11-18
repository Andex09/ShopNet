using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ShopNet.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HS256 signing key. Must be at least 256 bits. Never stored in appsettings: user-secrets locally, Key Vault in Azure.</summary>
    [Required]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>Who issues the token (this API). Validated on every request.</summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Who the token is for (the ShopNet clients). Validated on every request.</summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(5, 120)]
    public int LifetimeMinutes { get; set; } = 60;

    public byte[] GetSigningKeyBytes() => Encoding.UTF8.GetBytes(SigningKey);

    internal static bool HasValidKeyLength(JwtOptions options) =>
        Encoding.UTF8.GetByteCount(options.SigningKey) >= 32;
}
