using ShopNet.Domain.Orders;

namespace ShopNet.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken);

    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> AnyForProductAsync(int productId, CancellationToken cancellationToken);

    Task<bool> AnyForUserAsync(int userId, CancellationToken cancellationToken);

    void Add(Order order);

    void Remove(Order order);
}
