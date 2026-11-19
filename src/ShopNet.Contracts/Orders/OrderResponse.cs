namespace ShopNet.Contracts.Orders;

/// <summary>An order. All amounts are calculated by the server.</summary>
/// <param name="Status">One of <see cref="OrderStatusNames"/>.</param>
public sealed record OrderResponse(
    int Id,
    int UserId,
    DateTimeOffset CreatedAt,
    string Status,
    IReadOnlyList<OrderLineResponse> Lines,
    decimal Subtotal,
    decimal TaxRate,
    decimal Tax,
    decimal Total);
