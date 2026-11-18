using ShopNet.Domain.Users;

namespace ShopNet.Application.Abstractions;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> ListAsync(CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <param name="normalizedUserName">Lower-case user name (see <see cref="User.NormalizeUserName"/>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<User?> GetByUserNameAsync(string normalizedUserName, CancellationToken cancellationToken);

    Task<bool> UserNameExistsAsync(string normalizedUserName, int? excludeUserId, CancellationToken cancellationToken);

    void Add(User user);

    void Remove(User user);
}
