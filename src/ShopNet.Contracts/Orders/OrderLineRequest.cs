using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

/// <summary>A product and how many. The price is never sent: the server takes it from the catalog.</summary>
public sealed class OrderLineRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
