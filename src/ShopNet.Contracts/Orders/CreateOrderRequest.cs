using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

/// <summary>Admin creates an order on behalf of a user.</summary>
public sealed class CreateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int UserId { get; set; }

    [MinLength(1), MaxLength(50)]
    public List<OrderLineRequest> Lines { get; set; } = [];
}
