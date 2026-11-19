using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

public sealed class CreateMyOrderRequest
{
    [MinLength(1), MaxLength(50)]
    public List<OrderLineRequest> Lines { get; set; } = [];
}
