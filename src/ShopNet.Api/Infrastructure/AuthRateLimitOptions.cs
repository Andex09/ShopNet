using System.ComponentModel.DataAnnotations;

namespace ShopNet.Api.Infrastructure;

/// <summary>Fixed-window limit for /api/auth/* per client IP, against brute force and mass registration.</summary>
public sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting:Auth";

    [Range(1, 100_000)]
    public int PermitLimit { get; set; } = 10;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;
}
