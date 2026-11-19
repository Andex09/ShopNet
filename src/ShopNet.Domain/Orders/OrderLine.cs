using ShopNet.Domain.Common;
using ShopNet.Domain.Products;

namespace ShopNet.Domain.Orders;

/// <summary>
/// One product in an order. Name and unit price are copied when the line is created, so the order keeps what the
/// customer saw even if the product is renamed, repriced or deactivated later.
/// </summary>
public sealed class OrderLine
{
    private OrderLine()
    {
        ProductName = string.Empty;
    }

    public int Id { get; private set; }

    public int ProductId { get; private set; }

    public string ProductName { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineTotal { get; private set; }

    internal static OrderLine Create(Product product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (!product.IsActive)
        {
            throw new DomainException($"Product '{product.Name}' is no longer available.");
        }

        var line = new OrderLine
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
        };
        line.SetQuantity(quantity);
        return line;
    }

    internal void SetQuantity(int quantity)
    {
        if (quantity is <= 0 or > Order.MaxQuantityPerLine)
        {
            throw new DomainException($"Quantity must be between 1 and {Order.MaxQuantityPerLine}.");
        }

        Quantity = quantity;
        LineTotal = Math.Round(UnitPrice * quantity, 2, MidpointRounding.AwayFromZero);
    }
}
