using ShopNet.Domain.Products;

namespace ShopNet.Application.Abstractions;

public interface IProductRepository
{
    /// <summary>Products with their category, optionally filtered by category. Inactive ones only when asked.</summary>
    Task<IReadOnlyList<Product>> ListAsync(bool includeInactive, int? categoryId, CancellationToken cancellationToken);

    /// <summary>The product with its category, active or not.</summary>
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>The products with these ids that exist (missing ids are simply absent from the result).</summary>
    Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);

    Task<bool> AnyInCategoryAsync(int categoryId, CancellationToken cancellationToken);

    void Add(Product product);

    void Remove(Product product);
}
