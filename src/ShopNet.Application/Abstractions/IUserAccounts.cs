namespace ShopNet.Application.Abstractions;

/// <summary>
/// User accounts, roles and credentials. Implemented with ASP.NET Core Identity in Infrastructure:
/// password hashing, lockout and role storage live there, not in the domain.
/// </summary>
public interface IUserAccounts
{
    Task<IReadOnlyList<UserAccount>> ListAsync(CancellationToken cancellationToken);

    Task<UserAccount?> FindByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);

    /// <exception cref="Common.ConflictException">The user name is taken.</exception>
    Task<UserAccount> CreateAsync(string userName, string password, string role, CancellationToken cancellationToken);

    /// <summary>Changes the user name and role, and the password when <paramref name="newPassword"/> is set.</summary>
    /// <exception cref="Common.NotFoundException">The user does not exist.</exception>
    /// <exception cref="Common.ConflictException">The new user name is taken.</exception>
    Task UpdateAsync(int id, string userName, string role, string? newPassword, CancellationToken cancellationToken);

    /// <exception cref="Common.NotFoundException">The user does not exist.</exception>
    Task DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>Verifies the password. Failures count towards the account lockout.</summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(string userName, string password, CancellationToken cancellationToken);
}
