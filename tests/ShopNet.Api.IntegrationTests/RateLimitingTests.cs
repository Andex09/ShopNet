using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using ShopNet.Contracts.Auth;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class RateLimitingTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Auth_requests_over_the_limit_get_429_with_retry_after()
    {
        // Its own host with a limit of 2, so the shared API keeps its high test limit.
        await using var limited = factory.WithWebHostBuilder(builder => builder.UseSetting("RateLimiting:Auth:PermitLimit", "2"));
        var client = limited.CreateClient();
        var request = new LoginRequest { UserName = "nobody", Password = "wrong-password" };

        (await client.PostAsJsonAsync("/api/auth/login", request, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync("/api/auth/login", request, Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var rejected = await client.PostAsJsonAsync("/api/auth/login", request, Ct);

        rejected.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        rejected.Headers.RetryAfter.ShouldNotBeNull();
        rejected.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task Other_endpoints_are_not_limited()
    {
        await using var limited = factory.WithWebHostBuilder(builder => builder.UseSetting("RateLimiting:Auth:PermitLimit", "1"));
        var client = limited.CreateClient();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            (await client.GetAsync("/health/live", Ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
            (await client.GetAsync("/api/products", Ct)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    }
}
