using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Categories;

/// <summary>Body for creating or renaming a category.</summary>
public sealed class CategoryRequest
{
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
