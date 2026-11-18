using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

/// <summary>The signed-in user creates an order for themselves.</summary>
public sealed class CreateMyOrderRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
