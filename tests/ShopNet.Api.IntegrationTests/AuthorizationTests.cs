using System.Net;
using System.Net.Http.Json;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;

namespace ShopNet.Api.IntegrationTests;

/// <summary>Who can call what: anonymous, Customer and Admin.</summary>
[Collection(SharedApi.Name)]
public class AuthorizationTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<string, string> AdminEndpoints => new()
    {
        { "GET", "/api/users" },
        { "GET", "/api/users/1" },
        { "DELETE", "/api/users/1" },
        { "GET", "/api/orders" },
        { "POST", "/api/products" },
        { "PUT", "/api/products/1" },
        { "DELETE", "/api/products/1" },
        { "POST", "/api/categories" },
        { "PUT", "/api/categories/1" },
        { "DELETE", "/api/categories/1" },
    };

    [Theory]
    [InlineData("/api/products")]
    [InlineData("/api/users")]
    [InlineData("/api/orders")]
    [InlineData("/api/me/orders")]
    public async Task Anonymous_requests_are_rejected(string path)
    {
        var response = await factory.CreateClient().GetAsync(path, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [MemberData(nameof(AdminEndpoints))]
    public async Task Customers_cannot_use_admin_endpoints(string method, string path)
    {
        var client = await factory.CreateCustomerClientAsync();

        using var request = new HttpRequestMessage(new HttpMethod(method), path)
        {
            Content = method is "POST" or "PUT" ? JsonContent.Create(new ProductRequest { Name = "Hack", Price = 1 }) : null,
        };
        var response = await client.SendAsync(request, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/orders")]
    public async Task Admins_can_use_admin_endpoints(string path)
    {
        var client = await factory.CreateAdminClientAsync();

        var response = await client.GetAsync(path, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Customers_only_see_their_own_orders()
    {
        var alice = await factory.CreateCustomerClientAsync("alice");
        var bob = await factory.CreateCustomerClientAsync("bob");

        var created = await alice.PostAsJsonAsync("/api/me/orders", new CreateMyOrderRequest { ProductId = 2, Quantity = 1 }, Ct);
        var aliceOrder = await created.Content.ReadFromJsonAsync<OrderResponse>(Ct);

        var bobOrders = await bob.GetFromJsonAsync<List<OrderResponse>>("/api/me/orders", Ct);
        var aliceOrders = await alice.GetFromJsonAsync<List<OrderResponse>>("/api/me/orders", Ct);

        aliceOrder.ShouldNotBeNull();
        bobOrders.ShouldNotBeNull().ShouldNotContain(o => o.Id == aliceOrder.Id);
        aliceOrders.ShouldNotBeNull().ShouldContain(o => o.Id == aliceOrder.Id);
        aliceOrders.ShouldAllBe(o => o.UserId == aliceOrder.UserId);
    }
}
