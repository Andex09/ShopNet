using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Users;

namespace ShopNet.Application.Features.Users;

public sealed class GetUsersHandler(IUserAccounts users)
{
    public async Task<IReadOnlyList<UserResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await users.ListAsync(cancellationToken);
        return [.. items.Select(u => u.ToResponse())];
    }
}

public sealed class GetUserByIdHandler(IUserAccounts users)
{
    public async Task<UserResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("User", id);
        return user.ToResponse();
    }
}

public sealed class CreateUserHandler(IUserAccounts users, ILogger<CreateUserHandler> logger)
{
    public async Task<UserResponse> HandleAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.CreateAsync(request.UserName, request.Password, request.Role, cancellationToken);

        logger.UserCreated(user.Id, user.Role);
        return user.ToResponse();
    }
}

public sealed class UpdateUserHandler(IUserAccounts users)
{
    public Task HandleAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        return users.UpdateAsync(id, request.UserName, request.Role, request.Password, cancellationToken);
    }
}

public sealed class DeleteUserHandler(
    IUserAccounts users,
    IOrderRepository orders,
    ILogger<DeleteUserHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        if (!await users.ExistsAsync(id, cancellationToken))
        {
            throw new NotFoundException("User", id);
        }

        if (await orders.AnyForUserAsync(id, cancellationToken))
        {
            throw new ConflictException("The user has orders and cannot be deleted.");
        }

        await users.DeleteAsync(id, cancellationToken);
        logger.UserDeleted(id);
    }
}
