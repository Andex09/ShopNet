namespace ShopNet.Application.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ITokenService
{
    string CreateToken(UserAccount user);
}

/// <summary>The authenticated caller, resolved from the JWT.</summary>
public interface ICurrentUser
{
    /// <summary>The user id, or <c>null</c> for anonymous requests.</summary>
    int? UserId { get; }
}
