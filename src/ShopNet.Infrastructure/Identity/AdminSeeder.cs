using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Users;

namespace ShopNet.Infrastructure.Identity;

/// <summary>Creates the configured Admin if no user has that name. Never changes an existing account.</summary>
internal sealed partial class AdminSeeder(
    IOptions<AdminSeedOptions> options,
    UserManager<AppUser> userManager,
    IUserAccounts accounts,
    ILogger<AdminSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var seed = options.Value;
        if (!seed.IsConfigured)
        {
            return;
        }

        if (await userManager.FindByNameAsync(seed.UserName!) is not null)
        {
            return;
        }

        var admin = await accounts.CreateAsync(seed.UserName!, seed.Password!, Roles.Admin, cancellationToken);
        LogAdminCreated(logger, admin.Id, admin.UserName);
    }

    [LoggerMessage(EventId = 2000, Level = LogLevel.Information, Message = "Seeded admin user {UserId} ({UserName})")]
    private static partial void LogAdminCreated(ILogger logger, int userId, string userName);
}
