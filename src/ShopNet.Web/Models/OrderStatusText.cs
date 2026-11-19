using ShopNet.Contracts.Orders;

namespace ShopNet.Web.Models;

/// <summary>Spanish label and badge style for each order status the API returns.</summary>
public static class OrderStatusText
{
    public static string Label(string status) => status switch
    {
        OrderStatusNames.Pending => "Pendiente",
        OrderStatusNames.Confirmed => "Confirmado",
        OrderStatusNames.Cancelled => "Cancelado",
        _ => status,
    };

    public static string BadgeClass(string status) => status switch
    {
        OrderStatusNames.Pending => "text-bg-warning",
        OrderStatusNames.Confirmed => "text-bg-success",
        _ => "text-bg-secondary",
    };
}
