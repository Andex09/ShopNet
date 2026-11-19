using ShopNet.Domain.Categories;

namespace ShopNet.Application.Abstractions;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken);

    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string name, int? excludeCategoryId, CancellationToken cancellationToken);

    void Add(Category category);

    void Remove(Category category);
}
