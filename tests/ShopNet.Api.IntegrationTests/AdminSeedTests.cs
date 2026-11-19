using System.Net.Http.Json;
using ShopNet.Contracts.Users;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class AdminSeedTests(ApiFactory factory)
{
    [Fact]
    public async Task The_configured_admin_is_created_on_startup_and_can_sign_in()
    {
        var session = await factory.CreateClient().SignInAsync(ApiFactory.AdminUserName, ApiFactory.AdminPassword);

        session.User.UserName.ShouldBe(ApiFactory.AdminUserName);
        session.User.Role.ShouldBe(RoleNames.Admin);
    }

    [Fact]
    public async Task Restarting_the_api_does_not_create_a_second_admin()
    {
        // A second host on the same database runs the seed again.
        await using var restarted = factory.WithWebHostBuilder(_ => { });
        var client = restarted.CreateClient();
        await client.SignInAsync(ApiFactory.AdminUserName, ApiFactory.AdminPassword);

        var users = await client.GetFromJsonAsync<List<UserResponse>>("/api/users", TestContext.Current.CancellationToken);

        users.ShouldNotBeNull().Count(u => u.UserName == ApiFactory.AdminUserName).ShouldBe(1);
    }
}
