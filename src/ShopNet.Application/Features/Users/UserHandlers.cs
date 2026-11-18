using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Users;

namespace ShopNet.Application.Features.Users;

public sealed class GetUsersHandler(IUserRepository users)
{
    public async Task<IReadOnlyList<UserResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await users.ListAsync(cancellationToken);
        return [.. items.Select(u => u.ToResponse())];
    }
}

public sealed class GetUserByIdHandler(IUserRepository users)
{
    public async Task<UserResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);
        return user.ToResponse();
    }
}

public sealed class CreateUserHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<CreateUserHandler> logger)
{
    public async Task<UserResponse> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await CreateAsync(users, passwordHasher, request.UserName, request.Password, request.Role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserCreated(user.Id, user.Role);
        return user.ToResponse();
    }

    /// <summary>Shared by admin user creation and self-registration.</summary>
    internal static async Task<User> CreateAsync(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        string userName,
        string password,
        string role,
        CancellationToken cancellationToken)
    {
        var normalized = User.NormalizeUserName(userName);
        if (await users.UserNameExistsAsync(normalized, excludeUserId: null, cancellationToken))
        {
            throw new ConflictException("A user with that name already exists.");
        }

        var (hash, salt) = passwordHasher.HashPassword(password);
        var user = User.Create(normalized, role, hash, salt);
        users.Add(user);
        return user;
    }
}

public sealed class UpdateUserHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);

        var normalized = User.NormalizeUserName(request.UserName);
        if (await users.UserNameExistsAsync(normalized, excludeUserId: id, cancellationToken))
        {
            throw new ConflictException("A user with that name already exists.");
        }

        user.Update(normalized, request.Role);

        if (!string.IsNullOrEmpty(request.Password))
        {
            var (hash, salt) = passwordHasher.HashPassword(request.Password);
            user.SetPassword(hash, salt);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteUserHandler(
    IUserRepository users,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    ILogger<DeleteUserHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), id);

        if (await orders.AnyForUserAsync(id, cancellationToken))
        {
            throw new ConflictException("The user has orders and cannot be deleted.");
        }

        users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.UserDeleted(id);
    }
}
