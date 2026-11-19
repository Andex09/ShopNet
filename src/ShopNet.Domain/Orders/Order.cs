using ShopNet.Domain.Common;
using ShopNet.Domain.Products;

namespace ShopNet.Domain.Orders;

/// <summary>
/// An order for a single product. Amounts are always calculated here from the product price at the time of
/// the order, never taken from the client. (Multiple lines per order arrive in phase 4.)
/// </summary>
public sealed class Order
{
    public const int MaxQuantity = 1000;

    private Order() { }

    public int Id { get; private set; }

    public int UserId { get; private set; }

    public int ProductId { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Subtotal { get; private set; }

    /// <summary>Tax rate applied, e.g. 0.19 for 19%.</summary>
    public decimal TaxRate { get; private set; }

    /// <summary>Tax amount.</summary>
    public decimal Tax { get; private set; }

    public decimal Total { get; private set; }

    public static Order Create(int userId, Product product, int quantity, decimal taxRate)
    {
        if (userId <= 0)
        {
            throw new DomainException("A valid user is required.");
        }

        var order = new Order { UserId = userId };
        order.Change(product, quantity, taxRate);
        return order;
    }

    public void Change(Product product, int quantity, decimal taxRate)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (quantity is <= 0 or > MaxQuantity)
        {
            throw new DomainException($"Quantity must be between 1 and {MaxQuantity}.");
        }

        if (!product.IsActive)
        {
            throw new DomainException($"Product '{product.Name}' is no longer available.");
        }

        if (taxRate is < 0 or >= 1)
        {
            throw new DomainException("Tax rate must be between 0 and 1.");
        }

        ProductId = product.Id;
        UnitPrice = product.Price;
        Quantity = quantity;
        TaxRate = taxRate;
        Subtotal = Math.Round(UnitPrice * quantity, 2, MidpointRounding.AwayFromZero);
        Tax = Math.Round(Subtotal * taxRate, 2, MidpointRounding.AwayFromZero);
        Total = Subtotal + Tax;
    }
}
