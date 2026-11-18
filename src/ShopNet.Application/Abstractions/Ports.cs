using ShopNet.Domain.Users;

namespace ShopNet.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    (byte[] Hash, byte[] Salt) HashPassword(string password);

    bool VerifyPassword(string password, byte[] hash, byte[] salt);
}

public interface ITokenService
{
    string CreateToken(User user);
}

/// <summary>The authenticated caller, resolved from the JWT.</summary>
public interface ICurrentUser
{
    /// <summary>The user id, or <c>null</c> for anonymous requests.</summary>
    int? UserId { get; }
}
