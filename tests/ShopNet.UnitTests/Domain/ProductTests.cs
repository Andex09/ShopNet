using ShopNet.Domain.Common;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_trims_the_name()
    {
        var product = Product.Create("  Teclado  ", 100m);

        product.Name.ShouldBe("Teclado");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_rejects_non_positive_price(int price) =>
        Should.Throw<DomainException>(() => Product.Create("Teclado", price));

    [Fact]
    public void Create_rejects_empty_name() =>
        Should.Throw<DomainException>(() => Product.Create(" ", 100m));
}
