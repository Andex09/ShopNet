using ShopNet.Contracts.Orders;

namespace ShopNet.Api.IntegrationTests;

internal static class TestOrders
{
    public static CreateMyOrderRequest Of(params (int ProductId, int Quantity)[] lines) =>
        new() { Lines = [.. lines.Select(l => new OrderLineRequest { ProductId = l.ProductId, Quantity = l.Quantity })] };

    public static CreateMyOrderRequest Of(int productId, int quantity) => Of((productId, quantity));
}
