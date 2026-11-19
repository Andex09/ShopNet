using ShopNet.Domain.Categories;
using ShopNet.Domain.Common;

namespace ShopNet.Domain.Products;

public sealed class Product
{
    public const int NameMaxLength = 100;
    public const int DescriptionMaxLength = 1000;

    private Product()
    {
        Name = string.Empty;
        Description = string.Empty;
        Category = null!;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    /// <summary>What the product is and what it is for. Also the text that semantic search will index (phase 6).</summary>
    public string Description { get; private set; }

    public decimal Price { get; private set; }

    public int CategoryId { get; private set; }

    public Category Category { get; private set; }

    /// <summary>Inactive products stay in the history of past orders but cannot be ordered again.</summary>
    public bool IsActive { get; private set; }

    public static Product Create(string name, string? description, decimal price, Category category)
    {
        var product = new Product { IsActive = true };
        product.Update(name, description, price, category);
        return product;
    }

    public void Update(string name, string? description, decimal price, Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Product name is required.");
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
        {
            throw new DomainException($"Product name cannot exceed {NameMaxLength} characters.");
        }

        var trimmedDescription = description?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            throw new DomainException($"Product description cannot exceed {DescriptionMaxLength} characters.");
        }

        if (price <= 0)
        {
            throw new DomainException("Product price must be greater than zero.");
        }

        Name = trimmedName;
        Description = trimmedDescription;
        Price = price;
        Category = category;
        CategoryId = category.Id;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
