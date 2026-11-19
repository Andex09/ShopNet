using Microsoft.EntityFrameworkCore;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.Infrastructure.Persistence;

internal sealed class ProductRepository(ShopNetDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken) =>
        await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);
}

internal sealed class OrderRepository(ShopNetDbContext db) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().OrderByDescending(o => o.Id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().Where(o => o.UserId == userId).OrderByDescending(o => o.Id).ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<bool> AnyForProductAsync(int productId, CancellationToken cancellationToken) =>
        db.Orders.AnyAsync(o => o.ProductId == productId, cancellationToken);

    public Task<bool> AnyForUserAsync(int userId, CancellationToken cancellationToken) =>
        db.Orders.AnyAsync(o => o.UserId == userId, cancellationToken);

    public void Add(Order order) => db.Orders.Add(order);

    public void Remove(Order order) => db.Orders.Remove(order);
}
