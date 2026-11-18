using ShopNet.Domain.Common;
using ShopNet.Domain.Users;

namespace ShopNet.UnitTests.Domain;

public class UserTests
{
    private static readonly byte[] Hash = [1, 2, 3];
    private static readonly byte[] Salt = [4, 5, 6];

    [Fact]
    public void Create_normalizes_the_user_name()
    {
        var user = User.Create("  Valeria.D ", Roles.Basic, Hash, Salt);

        user.UserName.ShouldBe("valeria.d");
    }

    [Fact]
    public void Create_rejects_unknown_roles() =>
        Should.Throw<DomainException>(() => User.Create("ana", "SuperAdmin", Hash, Salt));
}
