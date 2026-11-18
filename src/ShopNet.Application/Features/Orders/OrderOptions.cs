using System.ComponentModel.DataAnnotations;

namespace ShopNet.Application.Features.Orders;

public sealed class OrderOptions
{
    public const string SectionName = "Orders";

    /// <summary>Tax rate (IVA) applied to new orders. 0.19 = 19%.</summary>
    [Range(typeof(decimal), "0", "0.99", ParseLimitsInInvariantCulture = true)]
    public decimal TaxRate { get; set; } = 0.19m;
}
