using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ShopNet.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>HS256 signing key. Must be at least 256 bits. Never stored in appsettings: user-secrets locally, Key Vault in Azure.</summary>
    [Required]
    public string SigningKey { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int LifetimeMinutes { get; set; } = 1440;

    public byte[] GetSigningKeyBytes() => Encoding.UTF8.GetBytes(SigningKey);

    internal static bool HasValidKeyLength(JwtOptions options) =>
        Encoding.UTF8.GetByteCount(options.SigningKey) >= 32;
}
