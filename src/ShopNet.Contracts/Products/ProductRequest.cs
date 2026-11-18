using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Products;

/// <summary>Body for creating or updating a product.</summary>
public sealed class ProductRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "100000000", ParseLimitsInInvariantCulture = true)]
    public decimal Price { get; set; }
}
