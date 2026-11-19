using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

/// <summary>Admin replaces the lines of a pending order.</summary>
public sealed class UpdateOrderRequest
{
    [MinLength(1), MaxLength(50)]
    public List<OrderLineRequest> Lines { get; set; } = [];
}
