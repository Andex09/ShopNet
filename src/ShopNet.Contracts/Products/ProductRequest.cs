using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Products;

/// <summary>Body for creating or updating a product.</summary>
public sealed class ProductRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "100000000", ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    /// <summary>Inactive products are hidden from the catalog and cannot be ordered.</summary>
    public bool IsActive { get; set; } = true;
}
