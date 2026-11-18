using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Features.Users;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Users;
using ShopNet.Application.Common;

namespace ShopNet.Application.Features.Auth;

/// <summary>Self-registration. New accounts always get the Customer role.</summary>
public sealed class RegisterHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ILogger<RegisterHandler> logger)
{
    public async Task<UserResponse> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await CreateUserHandler.CreateAsync(
            users, passwordHasher, request.UserName, request.Password, Roles.Customer, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.UserRegistered(user.Id);
        return user.ToResponse();
    }
}

public sealed class LoginHandler(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ILogger<LoginHandler> logger)
{
    /// <returns>The token and user, or <c>null</c> when the credentials are invalid.</returns>
    public async Task<LoginResponse?> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.GetByUserNameAsync(User.NormalizeUserName(request.UserName), cancellationToken);
        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            logger.LoginFailed(request.UserName);
            return null;
        }

        logger.LoginSucceeded(user.Id);
        return new LoginResponse(tokenService.CreateToken(user), user.ToResponse());
    }
}
