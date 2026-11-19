namespace ShopNet.Domain.Orders;

/// <summary>Pending → Confirmed, or Pending → Cancelled. Confirmed and cancelled orders no longer change.</summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled,
}
