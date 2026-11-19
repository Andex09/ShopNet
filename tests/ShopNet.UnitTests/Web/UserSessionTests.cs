using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ShopNet.Web.Auth;

namespace ShopNet.UnitTests.Web;

public class UserSessionTests
{
    private static readonly DateTimeOffset Expiry = new(2026, 11, 19, 13, 0, 0, TimeSpan.Zero);
    private static readonly string[] TwoRoles = ["Admin", "Customer"];

    [Fact]
    public void Reads_the_user_and_expiry_from_the_payload()
    {
        var token = Token(new { sub = "42", unique_name = "ana", role = "Customer", exp = Expiry.ToUnixTimeSeconds() });

        var session = UserSession.FromToken(token);

        session.ShouldNotBeNull();
        session.Token.ShouldBe(token);
        session.UserId.ShouldBe(42);
        session.UserName.ShouldBe("ana");
        session.Role.ShouldBe("Customer");
        session.ExpiresAt.ShouldBe(Expiry);
    }

    [Fact]
    public void Takes_the_first_role_when_the_claim_is_an_array()
    {
        var token = Token(new { sub = "1", unique_name = "admin", role = TwoRoles, exp = Expiry.ToUnixTimeSeconds() });

        UserSession.FromToken(token)?.Role.ShouldBe("Admin");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    [InlineData("a.b")]
    [InlineData("header.%%%.signature")]
    [InlineData("header.bm90LWpzb24.signature")] // payload is base64url("not-json")
    public void Rejects_malformed_tokens(string? token) =>
        UserSession.FromToken(token).ShouldBeNull();

    [Fact]
    public void Rejects_tokens_without_the_expected_claims() =>
        UserSession.FromToken(Token(new { sub = "42", exp = Expiry.ToUnixTimeSeconds() })).ShouldBeNull();

    [Fact]
    public void Is_expired_from_the_expiry_instant()
    {
        var session = UserSession.FromToken(Token(new { sub = "42", unique_name = "ana", role = "Customer", exp = Expiry.ToUnixTimeSeconds() }))!;

        session.IsExpired(Expiry.AddSeconds(-1)).ShouldBeFalse();
        session.IsExpired(Expiry).ShouldBeTrue();
    }

    [Fact]
    public void Principal_uses_the_standard_claim_types()
    {
        var principal = UserSession.FromToken(Token(new { sub = "42", unique_name = "ana", role = "Admin", exp = Expiry.ToUnixTimeSeconds() }))!
            .ToPrincipal();

        principal.Identity?.IsAuthenticated.ShouldBe(true);
        principal.FindFirstValue(ClaimTypes.NameIdentifier).ShouldBe("42");
        principal.Identity?.Name.ShouldBe("ana");
        principal.IsInRole("Admin").ShouldBeTrue();
    }

    /// <summary>An unsigned JWT with the given payload. The client never checks the signature.</summary>
    private static string Token(object payload) =>
        $"{Base64Url(new { alg = "HS256", typ = "JWT" })}.{Base64Url(payload)}.signature";

    private static string Base64Url(object value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
