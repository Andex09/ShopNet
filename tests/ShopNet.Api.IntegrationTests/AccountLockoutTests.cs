using System.Net;
using System.Net.Http.Json;
using ShopNet.Contracts.Auth;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class AccountLockoutTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Five_failed_logins_lock_the_account_even_for_the_right_password()
    {
        var client = factory.CreateClient();
        var userName = TestUsers.UniqueName("locked");
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { UserName = userName, Password = TestUsers.Password }, Ct);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            (await LoginAsync(client, userName, "wrong-password")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        var response = await LoginAsync(client, userName, TestUsers.Password);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task A_successful_login_resets_the_failure_count()
    {
        var client = factory.CreateClient();
        var userName = TestUsers.UniqueName("forgetful");
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { UserName = userName, Password = TestUsers.Password }, Ct);

        for (var round = 0; round < 2; round++)
        {
            for (var attempt = 0; attempt < 4; attempt++)
            {
                await LoginAsync(client, userName, "wrong-password");
            }

            (await LoginAsync(client, userName, TestUsers.Password)).StatusCode.ShouldBe(HttpStatusCode.OK);
        }
    }

    private static Task<HttpResponseMessage> LoginAsync(HttpClient client, string userName, string password) =>
        client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = userName, Password = password }, Ct);
}
