namespace ShopNet.Contracts.Orders;

/// <summary>An order. All amounts are calculated by the server.</summary>
public sealed record OrderResponse(
    int Id,
    int UserId,
    int ProductId,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal,
    decimal TaxRate,
    decimal Tax,
    decimal Total);
