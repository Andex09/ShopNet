using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using ShopNet.Application.Abstractions;
using ShopNet.Infrastructure.Security;

namespace ShopNet.UnitTests.Infrastructure;

public class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    private readonly JwtTokenService _service = new(
        Options.Create(new JwtOptions
        {
            SigningKey = "unit-tests-only-signing-key-0123456789abcdef",
            Issuer = "shopnet-api",
            Audience = "shopnet-web",
            LifetimeMinutes = 60,
        }),
        new FixedTimeProvider(Now));

    [Fact]
    public void Token_carries_the_user_claims()
    {
        var token = Read(_service.CreateToken(new UserAccount(42, "ana", "Customer")));

        token.Subject.ShouldBe("42");
        token.GetClaim(JwtRegisteredClaimNames.UniqueName).Value.ShouldBe("ana");
        token.GetClaim("role").Value.ShouldBe("Customer");
    }

    [Fact]
    public void Token_carries_issuer_audience_and_lifetime()
    {
        var token = Read(_service.CreateToken(new UserAccount(1, "admin", "Admin")));

        token.Issuer.ShouldBe("shopnet-api");
        token.Audiences.ShouldBe(["shopnet-web"]);
        token.IssuedAt.ShouldBe(Now.UtcDateTime);
        token.ValidTo.ShouldBe(Now.AddMinutes(60).UtcDateTime);
        token.Alg.ShouldBe("HS256");
    }

    [Fact]
    public void Every_token_has_its_own_id()
    {
        var user = new UserAccount(1, "admin", "Admin");

        var first = Read(_service.CreateToken(user)).Id;
        var second = Read(_service.CreateToken(user)).Id;

        first.ShouldNotBeNullOrEmpty();
        second.ShouldNotBe(first);
    }

    private static JsonWebToken Read(string token) => new JsonWebTokenHandler().ReadJsonWebToken(token);
}
