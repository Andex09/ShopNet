using ShopNet.Domain.Products;

namespace ShopNet.Application.Abstractions;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    void Add(Product product);

    void Remove(Product product);
}
