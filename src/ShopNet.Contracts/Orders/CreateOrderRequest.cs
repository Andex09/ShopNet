using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

/// <summary>Admin creates an order on behalf of a user. Prices are never sent by the client.</summary>
public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
