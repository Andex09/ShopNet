using System.Net;
using System.Net.Http.Headers;

namespace ShopNet.Api.IntegrationTests;

/// <summary>The API accepts only tokens it issued: right key, issuer and audience, and not expired.</summary>
[Collection(SharedApi.Name)]
public class TokenValidationTests(ApiFactory factory)
{
    [Fact]
    public async Task A_token_signed_by_the_api_is_accepted()
    {
        var response = await GetProductsAsync(TestTokens.Create());

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_token_from_another_issuer_is_rejected() =>
        (await GetProductsAsync(TestTokens.Create(issuer: "someone-else"))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task A_token_for_another_audience_is_rejected() =>
        (await GetProductsAsync(TestTokens.Create(audience: "another-app"))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task A_token_signed_with_another_key_is_rejected() =>
        (await GetProductsAsync(TestTokens.Create(signingKey: "attacker-signing-key-0123456789abcdef-xyz")))
            .StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    [Fact]
    public async Task An_expired_token_is_rejected() =>
        (await GetProductsAsync(TestTokens.Create(expiresIn: TimeSpan.FromMinutes(-5)))).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    private async Task<HttpResponseMessage> GetProductsAsync(string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await client.GetAsync("/api/products", TestContext.Current.CancellationToken);
    }
}
