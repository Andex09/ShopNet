using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Features.Auth;
using ShopNet.Contracts.Auth;

namespace ShopNet.UnitTests.Application;

public class LoginHandlerTests
{
    private static readonly LoginRequest Request = new() { UserName = "ana", Password = "S3cure-pass" };

    private readonly IUserAccounts _users = Substitute.For<IUserAccounts>();
    private readonly ITokenService _tokens = Substitute.For<ITokenService>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests() =>
        _handler = new LoginHandler(_users, _tokens, NullLogger<LoginHandler>.Instance);

    [Fact]
    public async Task Returns_the_token_and_user_when_the_credentials_are_valid()
    {
        var user = new UserAccount(7, "ana", "Customer");
        _users.CheckCredentialsAsync("ana", "S3cure-pass", Arg.Any<CancellationToken>())
            .Returns(new CredentialCheckResult(CredentialCheckStatus.Succeeded, user));
        _tokens.CreateToken(user).Returns("jwt");

        var response = await _handler.HandleAsync(Request, TestContext.Current.CancellationToken);

        response.ShouldNotBeNull();
        response.Token.ShouldBe("jwt");
        response.User.Id.ShouldBe(7);
        response.User.Role.ShouldBe("Customer");
    }

    [Theory]
    [InlineData(CredentialCheckStatus.InvalidCredentials)]
    [InlineData(CredentialCheckStatus.LockedOut)]
    public async Task Returns_null_without_a_token_when_the_login_is_rejected(CredentialCheckStatus status)
    {
        _users.CheckCredentialsAsync("ana", "S3cure-pass", Arg.Any<CancellationToken>())
            .Returns(new CredentialCheckResult(status));

        var response = await _handler.HandleAsync(Request, TestContext.Current.CancellationToken);

        response.ShouldBeNull();
        _tokens.DidNotReceive().CreateToken(Arg.Any<UserAccount>());
    }
}
