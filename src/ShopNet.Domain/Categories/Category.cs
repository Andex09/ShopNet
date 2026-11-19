using ShopNet.Domain.Common;

namespace ShopNet.Domain.Categories;

public sealed class Category
{
    public const int NameMaxLength = 50;

    private Category()
    {
        Name = string.Empty;
    }

    public int Id { get; private set; }

    public string Name { get; private set; }

    public static Category Create(string name)
    {
        var category = new Category();
        category.Rename(name);
        return category;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new DomainException($"Category name cannot exceed {NameMaxLength} characters.");
        }

        Name = trimmed;
    }
}
