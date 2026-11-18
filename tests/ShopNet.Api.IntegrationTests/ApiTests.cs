using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;

namespace ShopNet.Api.IntegrationTests;

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
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
    public async Task Anonymous_requests_are_rejected()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/products", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Basic_user_cannot_use_admin_endpoints()
    {
        var client = await CreateSignedInClientAsync("basic.user");

        var response = await client.PostAsJsonAsync("/api/products", new ProductRequest { Name = "Hack", Price = 1 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Order_totals_are_calculated_by_the_server()
    {
        var client = await CreateSignedInClientAsync("buyer");

        // Seeded product 2: "Mx 3 Mouse", 350.000. Tax 19%.
        var response = await client.PostAsJsonAsync("/api/me/orders", new CreateMyOrderRequest { ProductId = 2, Quantity = 2 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>(Ct);
        order.ShouldNotBeNull();
        order.UnitPrice.ShouldBe(350_000m);
        order.Subtotal.ShouldBe(700_000m);
        order.Tax.ShouldBe(133_000m);
        order.Total.ShouldBe(833_000m);

        var mine = await client.GetFromJsonAsync<List<OrderResponse>>("/api/me/orders", Ct);
        mine.ShouldNotBeNull();
        mine.ShouldContain(o => o.Id == order.Id);
    }

    [Fact]
    public async Task Unknown_product_returns_problem_details_without_internal_details()
    {
        var client = await CreateSignedInClientAsync("lost.buyer");

        var response = await client.PostAsJsonAsync("/api/me/orders", new CreateMyOrderRequest { ProductId = 999, Quantity = 1 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync(Ct);
        body.ShouldContain("traceId");
        body.ShouldNotContain("Exception");
        body.ShouldNotContain(" at ShopNet.");
    }

    private async Task<HttpClient> CreateSignedInClientAsync(string userName)
    {
        var client = factory.CreateClient();
        const string password = "Test-password-1";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { UserName = userName, Password = password }, Ct);
        register.StatusCode.ShouldBe(HttpStatusCode.Created);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = userName, Password = password }, Ct);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);
        var session = await login.Content.ReadFromJsonAsync<LoginResponse>(Ct);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session!.Token);
        return client;
    }
}
