using ShopNet.Domain.Common;

namespace ShopNet.Domain.Products;

public sealed class Product
{
    public const int NameMaxLength = 100;

    private Product()
    {
        Name = string.Empty;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public static Product Create(string name, decimal price)
    {
        var product = new Product();
        product.Update(name, price);
        return product;
    }

    public void Update(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new DomainException($"Product name cannot exceed {NameMaxLength} characters.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        Name = trimmed;
        Price = price;
    }
}
