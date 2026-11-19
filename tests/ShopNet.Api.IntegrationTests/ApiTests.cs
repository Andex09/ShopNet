using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Orders;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class ApiTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Readiness_probe_reports_healthy_when_the_database_is_up()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync(Ct)).ShouldBe("Healthy");
    }

    [Fact]
    public async Task Invalid_registration_returns_a_validation_problem()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { UserName = "a", Password = "123" }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        var errors = problem.RootElement.GetProperty("errors");
        errors.TryGetProperty("UserName", out _).ShouldBeTrue();
        errors.TryGetProperty("Password", out var passwordErrors).ShouldBeTrue();

        // Messages are English whatever the server culture is (this machine runs es-CO).
        (passwordErrors[0].GetString() ?? string.Empty).ShouldContain("characters");
    }

    [Fact]
    public async Task Order_totals_are_calculated_by_the_server()
    {
        var client = await factory.CreateCustomerClientAsync("buyer");

        // Seeded product 2: "Mx 3 Mouse", 350.000. Tax 19%.
        var response = await client.PostAsJsonAsync("/api/me/orders", new CreateMyOrderRequest { ProductId = 2, Quantity = 2 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(Ct);
        order.ShouldNotBeNull();
        order.UnitPrice.ShouldBe(350_000m);
        order.Subtotal.ShouldBe(700_000m);
        order.Tax.ShouldBe(133_000m);
        order.Total.ShouldBe(833_000m);
    }

    [Fact]
    public async Task Unknown_product_returns_problem_details_without_internal_details()
    {
        var client = await factory.CreateCustomerClientAsync("lost.buyer");

        var response = await client.PostAsJsonAsync("/api/me/orders", new CreateMyOrderRequest { ProductId = 999, Quantity = 1 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(Ct);
        body.ShouldContain("traceId");
        body.ShouldNotContain("Exception");
        body.ShouldNotContain(" at ShopNet.");
    }
}
