using Microsoft.EntityFrameworkCore;
using ShopNet.Application.Abstractions;
using ShopNet.Domain.Categories;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.Infrastructure.Persistence;

internal sealed class CategoryRepository(ShopNetDbContext db) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken) =>
        await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> NameExistsAsync(string name, int? excludeCategoryId, CancellationToken cancellationToken) =>
        db.Categories.AnyAsync(c => c.Name == name && c.Id != excludeCategoryId, cancellationToken);

    public void Add(Category category) => db.Categories.Add(category);

    public void Remove(Category category) => db.Categories.Remove(category);
}

internal sealed class ProductRepository(ShopNetDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> ListAsync(bool includeInactive, int? categoryId, CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (categoryId is { } id)
        {
            query = query.Where(p => p.CategoryId == id);
        }

        return await query.OrderBy(p => p.Category.Name).ThenBy(p => p.Name).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken) =>
        await db.Products.Include(p => p.Category).Where(p => ids.Contains(p.Id)).ToListAsync(cancellationToken);

    public Task<bool> AnyInCategoryAsync(int categoryId, CancellationToken cancellationToken) =>
        db.Products.AnyAsync(p => p.CategoryId == categoryId, cancellationToken);

    public void Add(Product product) => db.Products.Add(product);

    public void Remove(Product product) => db.Products.Remove(product);
}

internal sealed class OrderRepository(ShopNetDbContext db) : IOrderRepository
{
    public async Task<IReadOnlyList<Order>> ListAsync(CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().Include(o => o.Lines).AsSplitQuery()
            .OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListByUserAsync(int userId, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().Include(o => o.Lines).AsSplitQuery()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public Task<bool> AnyForProductAsync(int productId, CancellationToken cancellationToken) =>
        db.Set<OrderLine>().AnyAsync(l => l.ProductId == productId, cancellationToken);

    public Task<bool> AnyForUserAsync(int userId, CancellationToken cancellationToken) =>
        db.Orders.AnyAsync(o => o.UserId == userId, cancellationToken);

    public void Add(Order order) => db.Orders.Add(order);
}
