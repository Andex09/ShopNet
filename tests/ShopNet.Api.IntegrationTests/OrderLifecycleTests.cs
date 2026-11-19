using System.Net;
using System.Net.Http.Json;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class OrderLifecycleTests(ApiFactory factory)
{
    private const int Mouse = 2;
    private const int Headphones = 3;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task An_order_with_several_lines_merges_repeated_products_and_prices_them_on_the_server()
    {
        var customer = await factory.CreateCustomerClientAsync();
        var headphones = await customer.GetFromJsonAsync<ProductResponse>($"/api/products/{Headphones}", Ct);

        var order = await PlaceAsync(customer, (Mouse, 1), (Headphones, 2), (Mouse, 1));

        order.Lines.Count.ShouldBe(2);
        order.Lines.Single(l => l.ProductId == Mouse).Quantity.ShouldBe(2);
        var subtotal = 2 * 350_000m + 2 * headphones.ShouldNotBeNull().Price;
        order.Subtotal.ShouldBe(subtotal);
        order.Tax.ShouldBe(Math.Round(subtotal * 0.19m, 2, MidpointRounding.AwayFromZero));
        order.Total.ShouldBe(order.Subtotal + order.Tax);
    }

    [Fact]
    public async Task Prices_sent_by_the_client_are_ignored()
    {
        var customer = await factory.CreateCustomerClientAsync();

        var response = await customer.PostAsJsonAsync("/api/me/orders", new
        {
            lines = new[] { new { productId = Mouse, quantity = 1, unitPrice = 1m, lineTotal = 1m } },
            subtotal = 1m,
            total = 1m,
        }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var order = (await response.Content.ReadFromJsonAsync<OrderResponse>(Ct)).ShouldNotBeNull();
        order.Lines.ShouldHaveSingleItem().UnitPrice.ShouldBe(350_000m);
        order.Total.ShouldBe(416_500m);
    }

    [Fact]
    public async Task A_line_with_zero_quantity_is_a_validation_problem()
    {
        var customer = await factory.CreateCustomerClientAsync();

        var response = await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(Mouse, 0), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task The_admin_confirms_a_pending_order_and_then_it_cannot_change()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var order = await PlaceAsync(customer, (Mouse, 1));

        (await admin.PostAsync($"/api/orders/{order.Id}/confirm", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await StatusOfAsync(customer, order.Id)).ShouldBe(OrderStatusNames.Confirmed);
        (await admin.PostAsync($"/api/orders/{order.Id}/confirm", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsync($"/api/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PutAsJsonAsync($"/api/orders/{order.Id}", Update((Headphones, 1)), Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await customer.PostAsync($"/api/me/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task The_admin_changes_the_lines_of_a_pending_order_and_the_amounts_are_recalculated()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var order = await PlaceAsync(customer, (Headphones, 1));

        (await admin.PutAsJsonAsync($"/api/orders/{order.Id}", Update((Mouse, 3)), Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updated = await admin.GetFromJsonAsync<OrderResponse>($"/api/orders/{order.Id}", Ct);
        updated.ShouldNotBeNull().Lines.ShouldHaveSingleItem().ProductId.ShouldBe(Mouse);
        updated.Subtotal.ShouldBe(1_050_000m);
        updated.Total.ShouldBe(1_249_500m);
        updated.CreatedAt.ShouldBe(order.CreatedAt);
    }

    [Fact]
    public async Task The_admin_cancels_a_pending_order_and_then_it_cannot_be_confirmed()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var order = await PlaceAsync(customer, (Mouse, 1));

        (await admin.PostAsync($"/api/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await StatusOfAsync(customer, order.Id)).ShouldBe(OrderStatusNames.Cancelled);
        (await admin.PostAsync($"/api/orders/{order.Id}/confirm", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PostAsync("/api/orders/999999/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_customer_cancels_their_own_pending_order_once()
    {
        var customer = await factory.CreateCustomerClientAsync();
        var order = await PlaceAsync(customer, (Mouse, 1));

        (await customer.PostAsync($"/api/me/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await StatusOfAsync(customer, order.Id)).ShouldBe(OrderStatusNames.Cancelled);
        (await customer.PostAsync($"/api/me/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Someone_elses_order_is_not_found_and_cannot_be_cancelled()
    {
        var alice = await factory.CreateCustomerClientAsync("alice");
        var bob = await factory.CreateCustomerClientAsync("bob");
        var order = await PlaceAsync(alice, (Mouse, 1));

        (await bob.GetAsync($"/api/me/orders/{order.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await bob.PostAsync($"/api/me/orders/{order.Id}/cancel", null, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await StatusOfAsync(alice, order.Id)).ShouldBe(OrderStatusNames.Pending);
    }

    [Fact]
    public async Task The_admin_creates_an_order_for_a_customer()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var userId = (await PlaceAsync(customer, (Mouse, 1))).UserId;

        var response = await admin.PostAsJsonAsync("/api/orders", new CreateOrderRequest
        {
            UserId = userId,
            Lines = [new OrderLineRequest { ProductId = Headphones, Quantity = 1 }],
        }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var mine = await customer.GetFromJsonAsync<List<OrderResponse>>("/api/me/orders", Ct);
        mine.ShouldNotBeNull().Count.ShouldBe(2);
    }

    private static async Task<OrderResponse> PlaceAsync(HttpClient customer, params (int ProductId, int Quantity)[] lines)
    {
        var response = await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(lines), Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderResponse>(Ct)).ShouldNotBeNull();
    }

    private static async Task<string> StatusOfAsync(HttpClient customer, int orderId) =>
        (await customer.GetFromJsonAsync<OrderResponse>($"/api/me/orders/{orderId}", Ct)).ShouldNotBeNull().Status;

    private static UpdateOrderRequest Update(params (int ProductId, int Quantity)[] lines) => new()
    {
        Lines = [.. lines.Select(l => new OrderLineRequest { ProductId = l.ProductId, Quantity = l.Quantity })],
    };
}
