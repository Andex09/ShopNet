using ShopNet.Domain.Common;

namespace ShopNet.Domain.Orders;

/// <summary>The operation is not allowed in the order's current status (for example, confirming a cancelled order).</summary>
public sealed class OrderStateException : DomainException
{
    public OrderStateException(string message) : base(message) { }

    public OrderStateException() { }

    public OrderStateException(string message, Exception innerException) : base(message, innerException) { }
}
