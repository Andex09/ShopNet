using ShopNet.Domain.Common;
using ShopNet.Domain.Products;

namespace ShopNet.Domain.Orders;

/// <summary>
/// Order aggregate: the lines, the status and the amounts. Amounts are always calculated here from the stored product
/// prices, never taken from the client. Lines can only change while the order is pending.
/// </summary>
public sealed class Order
{
    public const int MaxQuantityPerLine = 1000;
    public const int MaxLines = 50;

    private readonly List<OrderLine> _lines = [];

    private Order() { }

    public int Id { get; private set; }

    public int UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyList<OrderLine> Lines => _lines;

    /// <summary>Tax rate applied, e.g. 0.19 for 19%. Fixed when the order is created.</summary>
    public decimal TaxRate { get; private set; }

    public decimal Subtotal { get; private set; }

    /// <summary>Tax amount, calculated once on the subtotal.</summary>
    public decimal Tax { get; private set; }

    public decimal Total { get; private set; }

    /// <param name="lines">Products and quantities. The same product twice becomes one line with the quantities added up.</param>
    public static Order Create(int userId, IEnumerable<(Product Product, int Quantity)> lines, decimal taxRate, DateTimeOffset createdAt)
    {
        if (userId <= 0)
        {
            throw new DomainException("A valid user is required.");
        }

        if (taxRate is < 0 or >= 1)
        {
            throw new DomainException("Tax rate must be between 0 and 1.");
        }

        var order = new Order
        {
            UserId = userId,
            TaxRate = taxRate,
            CreatedAt = createdAt,
            Status = OrderStatus.Pending,
        };
        order.SetLines(lines);
        return order;
    }

    public void ReplaceLines(IEnumerable<(Product Product, int Quantity)> lines)
    {
        EnsurePending("change the lines of");
        SetLines(lines);
    }

    public void Confirm()
    {
        EnsurePending("confirm");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        EnsurePending("cancel");
        Status = OrderStatus.Cancelled;
    }

    private void SetLines(IEnumerable<(Product Product, int Quantity)> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        var merged = new List<OrderLine>();
        foreach (var (product, quantity) in lines)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (merged.Find(l => l.ProductId == product.Id) is { } existing)
            {
                existing.SetQuantity(existing.Quantity + quantity);
            }
            else
            {
                merged.Add(OrderLine.Create(product, quantity));
            }
        }

        if (merged.Count == 0)
        {
            throw new DomainException("An order needs at least one line.");
        }

        if (merged.Count > MaxLines)
        {
            throw new DomainException($"An order cannot have more than {MaxLines} different products.");
        }

        _lines.Clear();
        _lines.AddRange(merged);

        Subtotal = _lines.Sum(l => l.LineTotal);
        Tax = Math.Round(Subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        Total = Subtotal + Tax;
    }

    private void EnsurePending(string action)
    {
        if (Status != OrderStatus.Pending)
        {
            var current = Status == OrderStatus.Confirmed ? "confirmed" : "cancelled";
            throw new OrderStateException($"Cannot {action} an order that is already {current}.");
        }
    }
}
