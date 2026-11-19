using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Users;

namespace ShopNet.Application.Features.Auth;

/// <summary>Self-registration. New accounts always get the Customer role.</summary>
public sealed class RegisterHandler(IUserAccounts users, ILogger<RegisterHandler> logger)
{
    public async Task<UserResponse> HandleAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await users.CreateAsync(request.UserName, request.Password, Roles.Customer, cancellationToken);

        logger.UserRegistered(user.Id);
        return user.ToResponse();
    }
}

public sealed class LoginHandler(
    IUserAccounts users,
    ITokenService tokenService,
    ILogger<LoginHandler> logger)
{
    /// <returns>
    /// The token and user, or <c>null</c> when the credentials are invalid or the account is locked out.
    /// Both look the same to the caller, so the response does not reveal which accounts exist.
    /// </returns>
    public async Task<LoginResponse?> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await users.CheckCredentialsAsync(request.UserName, request.Password, cancellationToken);
        switch (result)
        {
            case { Status: CredentialCheckStatus.Succeeded, User: { } user }:
                logger.LoginSucceeded(user.Id);
                return new LoginResponse(tokenService.CreateToken(user), user.ToResponse());

            case { Status: CredentialCheckStatus.LockedOut }:
                logger.LoginLockedOut(request.UserName);
                return null;

            default:
                logger.LoginFailed(request.UserName);
                return null;
        }
    }
}
