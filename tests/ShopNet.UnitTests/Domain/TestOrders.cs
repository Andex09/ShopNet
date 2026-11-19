using ShopNet.Domain.Orders;

namespace ShopNet.UnitTests.Domain;

internal static class TestOrders
{
    public static readonly DateTimeOffset CreatedAt = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);

    public static Order Pending(int userId = 7) =>
        Order.Create(userId, [(TestCatalog.Product("Mouse", 350_000m, id: 2), 1)], 0.19m, CreatedAt);
}
