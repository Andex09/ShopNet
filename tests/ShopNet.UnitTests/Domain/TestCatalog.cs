using ShopNet.Domain.Categories;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

/// <summary>Builds catalog entities for tests that only care about the product name and price.</summary>
internal static class TestCatalog
{
    public static readonly Category Peripherals = Category.Create("Periféricos");

    public static Product Product(string name, decimal price) =>
        ShopNet.Domain.Products.Product.Create(name, description: null, price, Peripherals);
}
