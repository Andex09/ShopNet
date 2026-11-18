using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Orders;

public sealed class UpdateOrderRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; } = 1;
}
