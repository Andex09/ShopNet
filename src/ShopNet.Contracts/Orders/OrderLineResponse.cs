namespace ShopNet.Contracts.Orders;

/// <summary>A line as it was ordered: name and unit price are the ones at purchase time.</summary>
public sealed record OrderLineResponse(int ProductId, string ProductName, decimal UnitPrice, int Quantity, decimal LineTotal);
