using ShopNet.Application.Abstractions;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.Application.Features;

/// <summary>Explicit entity → contract mapping (no reflection-based mapper).</summary>
public static class Mappings
{
    public static ProductResponse ToResponse(this Product product) =>
        new(product.Id, product.Name, product.Price);

    public static UserResponse ToResponse(this UserAccount user) =>
        new(user.Id, user.UserName, user.Role);

    public static OrderResponse ToResponse(this Order order) =>
        new(order.Id, order.UserId, order.ProductId, order.UnitPrice, order.Quantity,
            order.Subtotal, order.TaxRate, order.Tax, order.Total);
}
