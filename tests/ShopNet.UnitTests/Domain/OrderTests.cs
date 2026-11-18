using ShopNet.Domain.Common;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

public class OrderTests
{
    private static readonly Product Mouse = Product.Create("Mx 3 Mouse", 350_000m);

    [Fact]
    public void Create_calculates_amounts_from_the_product_price()
    {
        var order = Order.Create(userId: 7, Mouse, quantity: 2, taxRate: 0.19m);

        order.UserId.ShouldBe(7);
        order.UnitPrice.ShouldBe(350_000m);
        order.Quantity.ShouldBe(2);
        order.Subtotal.ShouldBe(700_000m);
        order.Tax.ShouldBe(133_000m);
        order.Total.ShouldBe(833_000m);
    }

    [Fact]
    public void Create_rounds_tax_to_two_decimals_away_from_zero()
    {
        var product = Product.Create("Cable", 10.05m);

        var order = Order.Create(1, product, quantity: 1, taxRate: 0.19m);

        // 10.05 * 0.19 = 1.9095 → 1.91
        order.Tax.ShouldBe(1.91m);
        order.Total.ShouldBe(11.96m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(Order.MaxQuantity + 1)]
    public void Create_rejects_quantity_out_of_range(int quantity) =>
        Should.Throw<DomainException>(() => Order.Create(1, Mouse, quantity, 0.19m));

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1)]
    public void Create_rejects_invalid_tax_rate(double taxRate) =>
        Should.Throw<DomainException>(() => Order.Create(1, Mouse, 1, (decimal)taxRate));

    [Fact]
    public void Change_recalculates_with_the_new_product_and_quantity()
    {
        var order = Order.Create(1, Mouse, 1, 0.19m);
        var headphones = Product.Create("Audifonos", 200_000m);

        order.Change(headphones, quantity: 3, taxRate: 0.19m);

        order.UnitPrice.ShouldBe(200_000m);
        order.Subtotal.ShouldBe(600_000m);
        order.Total.ShouldBe(714_000m);
    }
}
