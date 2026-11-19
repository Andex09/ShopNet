using ShopNet.Domain.Common;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

public class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly Product Mouse = TestCatalog.Product("Mouse", 350_000m, id: 2);
    private static readonly Product Headphones = TestCatalog.Product("Audífonos", 200_000m, id: 3);

    [Fact]
    public void Create_starts_pending_and_calculates_amounts_from_the_product_prices()
    {
        var order = Order.Create(userId: 7, [(Mouse, 2), (Headphones, 1)], taxRate: 0.19m, Now);

        order.UserId.ShouldBe(7);
        order.CreatedAt.ShouldBe(Now);
        order.Status.ShouldBe(OrderStatus.Pending);
        order.Lines.Count.ShouldBe(2);
        order.Subtotal.ShouldBe(900_000m);
        order.Tax.ShouldBe(171_000m);
        order.Total.ShouldBe(1_071_000m);
    }

    [Fact]
    public void Lines_copy_the_product_name_and_price_at_purchase_time()
    {
        var keyboard = TestCatalog.Product("Teclado", 100_000m, id: 9);
        var order = Order.Create(1, [(keyboard, 3)], 0.19m, Now);

        keyboard.Update("Teclado nuevo", null, 999_000m, TestCatalog.Peripherals);

        var line = order.Lines.ShouldHaveSingleItem();
        line.ProductId.ShouldBe(9);
        line.ProductName.ShouldBe("Teclado");
        line.UnitPrice.ShouldBe(100_000m);
        line.Quantity.ShouldBe(3);
        line.LineTotal.ShouldBe(300_000m);
    }

    [Fact]
    public void The_same_product_twice_becomes_one_line()
    {
        var order = Order.Create(1, [(Mouse, 1), (Mouse, 2)], 0.19m, Now);

        order.Lines.ShouldHaveSingleItem().Quantity.ShouldBe(3);
    }

    [Fact]
    public void Tax_is_rounded_once_on_the_subtotal()
    {
        var sticker = TestCatalog.Product("Sticker", 0.03m, id: 4);
        var label = TestCatalog.Product("Etiqueta", 0.03m, id: 5);

        var order = Order.Create(1, [(sticker, 1), (label, 1)], 0.19m, Now);

        // Per line: 0.0057 rounds up to 0.01 twice (0.02). On the subtotal: 0.06 * 0.19 = 0.0114 → 0.01.
        order.Subtotal.ShouldBe(0.06m);
        order.Tax.ShouldBe(0.01m);
        order.Total.ShouldBe(0.07m);
    }

    [Fact]
    public void Create_rejects_an_order_without_lines() =>
        Should.Throw<DomainException>(() => Order.Create(1, [], 0.19m, Now));

    [Fact]
    public void Create_rejects_too_many_different_products()
    {
        var lines = Enumerable.Range(1, Order.MaxLines + 1).Select(id => (TestCatalog.Product($"P{id}", 1m, id), 1));

        Should.Throw<DomainException>(() => Order.Create(1, lines, 0.19m, Now));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Order.MaxQuantityPerLine + 1)]
    public void Create_rejects_quantity_out_of_range(int quantity) =>
        Should.Throw<DomainException>(() => Order.Create(1, [(Mouse, quantity)], 0.19m, Now));

    [Fact]
    public void Merged_quantities_must_also_stay_in_range() =>
        Should.Throw<DomainException>(() => Order.Create(1, [(Mouse, 600), (Mouse, 600)], 0.19m, Now));

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1)]
    public void Create_rejects_invalid_tax_rate(double taxRate) =>
        Should.Throw<DomainException>(() => Order.Create(1, [(Mouse, 1)], (decimal)taxRate, Now));

    [Fact]
    public void Create_rejects_an_inactive_product()
    {
        var discontinued = TestCatalog.Product("Descontinuado", 1_000m, id: 8);
        discontinued.Deactivate();

        Should.Throw<DomainException>(() => Order.Create(1, [(discontinued, 1)], 0.19m, Now));
    }

    [Fact]
    public void ReplaceLines_recalculates_a_pending_order()
    {
        var order = Order.Create(1, [(Mouse, 1)], 0.19m, Now);

        order.ReplaceLines([(Headphones, 3)]);

        order.Lines.ShouldHaveSingleItem().ProductId.ShouldBe(3);
        order.Subtotal.ShouldBe(600_000m);
        order.Total.ShouldBe(714_000m);
    }

    [Fact]
    public void A_pending_order_can_be_confirmed_or_cancelled()
    {
        var confirmed = Order.Create(1, [(Mouse, 1)], 0.19m, Now);
        var cancelled = Order.Create(1, [(Mouse, 1)], 0.19m, Now);

        confirmed.Confirm();
        cancelled.Cancel();

        confirmed.Status.ShouldBe(OrderStatus.Confirmed);
        cancelled.Status.ShouldBe(OrderStatus.Cancelled);
    }

    [Fact]
    public void A_confirmed_order_cannot_change_or_be_cancelled()
    {
        var order = Order.Create(1, [(Mouse, 1)], 0.19m, Now);
        order.Confirm();

        Should.Throw<OrderStateException>(() => order.ReplaceLines([(Headphones, 1)]));
        Should.Throw<OrderStateException>(order.Cancel);
        Should.Throw<OrderStateException>(order.Confirm);
    }

    [Fact]
    public void A_cancelled_order_cannot_be_confirmed()
    {
        var order = Order.Create(1, [(Mouse, 1)], 0.19m, Now);
        order.Cancel();

        Should.Throw<OrderStateException>(order.Confirm);
    }
}
