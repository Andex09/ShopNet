using ShopNet.Domain.Categories;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

/// <summary>Builds catalog entities for tests that only care about the product name, price and id.</summary>
internal static class TestCatalog
{
    public static readonly Category Peripherals = Category.Create("Periféricos");

    /// <param name="id">The id the database would assign.</param>
    public static Product Product(string name, decimal price, int id = 0)
    {
        var product = ShopNet.Domain.Products.Product.Create(name, description: null, price, Peripherals);

        typeof(Product).GetProperty(nameof(ShopNet.Domain.Products.Product.Id))!.SetValue(product, id);
        return product;
    }
}
