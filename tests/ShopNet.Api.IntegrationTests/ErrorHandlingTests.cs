using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ShopNet.Application.Abstractions;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class ErrorHandlingTests(ApiFactory factory)
{
    private const string InternalDetail = "Login failed for user 'sa' on sql-internal-host";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task An_unexpected_error_is_a_generic_500_without_internal_details()
    {
        var categories = Substitute.For<ICategoryRepository>();
        categories.ListAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException(InternalDetail));
        await using var failing = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddScoped(_ => categories)));
        var client = failing.CreateClient();
        await client.SignInAsync(ApiFactory.AdminUserName, ApiFactory.AdminPassword);

        var response = await client.GetAsync("/api/categories", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadAsStringAsync(Ct);
        body.ShouldNotContain("sql-internal-host");
        body.ShouldNotContain(nameof(InvalidOperationException));
        using var problem = JsonDocument.Parse(body);
        problem.RootElement.GetProperty("title").GetString().ShouldBe("An unexpected error occurred");
        problem.RootElement.TryGetProperty("traceId", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task A_malformed_body_is_a_400_problem()
    {
        var client = await factory.CreateCustomerClientAsync();

        using var content = new StringContent("{ \"lines\": [ { \"productId\": ", Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/me/orders", content, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        (await response.Content.ReadAsStringAsync(Ct)).ShouldNotContain("JsonException");
    }
}
