using ShopNet.Application.Abstractions;
using ShopNet.Contracts.Categories;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Categories;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.Application.Features;

/// <summary>Explicit entity → contract mapping (no reflection-based mapper).</summary>
public static class Mappings
{
    public static CategoryResponse ToResponse(this Category category) =>
        new(category.Id, category.Name);

    /// <summary>Expects the category to be loaded (repositories include it).</summary>
    public static ProductResponse ToResponse(this Product product) =>
        new(product.Id, product.Name, product.Description, product.Price, product.CategoryId, product.Category.Name, product.IsActive);

    public static UserResponse ToResponse(this UserAccount user) =>
        new(user.Id, user.UserName, user.Role);

    public static OrderResponse ToResponse(this Order order) =>
        new(order.Id, order.UserId, order.CreatedAt, order.Status.ToString(),
            [.. order.Lines.Select(l => new OrderLineResponse(l.ProductId, l.ProductName, l.UnitPrice, l.Quantity, l.LineTotal))],
            order.Subtotal, order.TaxRate, order.Tax, order.Total);
}
