namespace ShopNet.Infrastructure.Identity;

/// <summary>
/// First Admin account, created on startup when it does not exist. The password is a secret:
/// user-secrets or the AppHost parameter locally, Key Vault in Azure.
/// </summary>
public sealed class AdminSeedOptions
{
    public const string SectionName = "Seed:Admin";

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(UserName);

    internal static bool HasPasswordWhenConfigured(AdminSeedOptions options) =>
        !options.IsConfigured || !string.IsNullOrEmpty(options.Password);
}
