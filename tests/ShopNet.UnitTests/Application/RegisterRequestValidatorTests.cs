using ShopNet.Application.Features;
using ShopNet.Contracts.Auth;

namespace ShopNet.UnitTests.Application;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Theory]
    [InlineData("ab", "longenough")]        // user name too short
    [InlineData("ana maria", "longenough")] // spaces not allowed
    [InlineData("ana", "short")]            // password too short
    public void Rejects_invalid_input(string userName, string password)
    {
        var result = _validator.Validate(new RegisterRequest { UserName = userName, Password = password });

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Accepts_valid_input()
    {
        var result = _validator.Validate(new RegisterRequest { UserName = "ana.maria", Password = "S3cure-pass" });

        result.IsValid.ShouldBeTrue();
    }
}
