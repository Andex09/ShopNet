namespace ShopNet.Web.Services;

public sealed record CartItem(int ProductId, string ProductName, decimal UnitPrice, int Quantity);
