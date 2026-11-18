using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Domain.Common;
using ShopNet.Domain.Users;
using ShopNet.Infrastructure.Persistence;

namespace ShopNet.Infrastructure.Identity;

/// <summary><see cref="IUserAccounts"/> on top of ASP.NET Core Identity. Every user has exactly one role.</summary>
internal sealed class UserAccounts(UserManager<AppUser> userManager, ShopNetDbContext db) : IUserAccounts
{
    public async Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken) =>
        await Accounts(db.Users).ToListAsync(cancellationToken);

    public Task<UserAccount?> FindByIdAsync(int id, CancellationToken cancellationToken) =>
        Accounts(db.Users.Where(u => u.Id == id)).FirstOrDefaultAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        db.Users.AnyAsync(u => u.Id == id, cancellationToken);

    public Task<UserAccount> CreateAsync(string userName, string password, string role, CancellationToken cancellationToken) =>
        InTransactionAsync(async () =>
        {
            var user = new AppUser { UserName = userName.Trim() };
            EnsureSucceeded(await userManager.CreateAsync(user, password));
            EnsureSucceeded(await userManager.AddToRoleAsync(user, role));
            return new UserAccount(user.Id, user.UserName, role);
        }, cancellationToken);

    public Task UpdateAsync(int id, string userName, string role, string? newPassword, CancellationToken cancellationToken) =>
        InTransactionAsync(async () =>
        {
            var user = await FindUserAsync(id);
            EnsureSucceeded(await userManager.SetUserNameAsync(user, userName.Trim()));

            var currentRoles = await userManager.GetRolesAsync(user);
            if (currentRoles is not [var currentRole] || currentRole != role)
            {
                EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, currentRoles));
                EnsureSucceeded(await userManager.AddToRoleAsync(user, role));
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (await userManager.HasPasswordAsync(user))
                {
                    EnsureSucceeded(await userManager.RemovePasswordAsync(user));
                }

                EnsureSucceeded(await userManager.AddPasswordAsync(user, newPassword));
            }

            return true;
        }, cancellationToken);

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var user = await FindUserAsync(id);
        EnsureSucceeded(await userManager.DeleteAsync(user));
    }

    public async Task<CredentialCheckResult> CheckCredentialsAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(userName.Trim());
        if (user is null)
        {
            return new CredentialCheckResult(CredentialCheckStatus.InvalidCredentials);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new CredentialCheckResult(CredentialCheckStatus.LockedOut);
        }

        // Also upgrades the stored hash when the hasher's parameters changed (PasswordVerificationResult.SuccessRehashNeeded).
        if (!await userManager.CheckPasswordAsync(user, password))
        {
            await userManager.AccessFailedAsync(user);
            return new CredentialCheckResult(await userManager.IsLockedOutAsync(user)
                ? CredentialCheckStatus.LockedOut
                : CredentialCheckStatus.InvalidCredentials);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? Roles.Customer;
        return new CredentialCheckResult(CredentialCheckStatus.Succeeded, new UserAccount(user.Id, user.UserName!, role));
    }

    /// <summary>User name and role in one query (filters go before the projection so EF can translate them).</summary>
    private IQueryable<UserAccount> Accounts(IQueryable<AppUser> users) =>
        from user in users.AsNoTracking()
        join userRole in db.UserRoles on user.Id equals userRole.UserId
        join role in db.Roles on userRole.RoleId equals role.Id
        orderby user.UserName
        select new UserAccount(user.Id, user.UserName!, role.Name!);

    private async Task<AppUser> FindUserAsync(int id) =>
        await userManager.FindByIdAsync(id.ToString(CultureInfo.InvariantCulture))
            ?? throw new NotFoundException("User", id);

    /// <summary>
    /// UserManager saves after every call; the transaction keeps user, role and password changes all-or-nothing.
    /// Runs through the execution strategy because the Aspire SQL Server integration enables retries.
    /// </summary>
    private Task<T> InTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async ct =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var result = await operation();
            await transaction.CommitAsync(ct);
            return result;
        }, cancellationToken);

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.DuplicateUserName)))
        {
            throw new ConflictException("A user with that name already exists.");
        }

        // Password and user name rules mirror the request validators, so this only happens if they drift apart.
        throw new DomainException(string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
