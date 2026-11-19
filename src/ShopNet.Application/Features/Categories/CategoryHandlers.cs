using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Categories;
using ShopNet.Domain.Categories;

namespace ShopNet.Application.Features.Categories;

public sealed class GetCategoriesHandler(ICategoryRepository categories)
{
    public async Task<IReadOnlyList<CategoryResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await categories.ListAsync(cancellationToken);
        return [.. items.Select(c => c.ToResponse())];
    }
}

public sealed class GetCategoryByIdHandler(ICategoryRepository categories)
{
    public async Task<CategoryResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);
        return category.ToResponse();
    }
}

public sealed class CreateCategoryHandler(
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ILogger<CreateCategoryHandler> logger)
{
    public async Task<CategoryResponse> HandleAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (await categories.NameExistsAsync(request.Name.Trim(), excludeCategoryId: null, cancellationToken))
        {
            throw new ConflictException("A category with that name already exists.");
        }

        var category = Category.Create(request.Name);
        categories.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.CategoryCreated(category.Id);
        return category.ToResponse();
    }
}

public sealed class UpdateCategoryHandler(ICategoryRepository categories, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, CategoryRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        if (await categories.NameExistsAsync(request.Name.Trim(), excludeCategoryId: id, cancellationToken))
        {
            throw new ConflictException("A category with that name already exists.");
        }

        category.Rename(request.Name);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteCategoryHandler(
    ICategoryRepository categories,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    ILogger<DeleteCategoryHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categories.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), id);

        if (await products.AnyInCategoryAsync(id, cancellationToken))
        {
            throw new ConflictException("The category has products. Move or delete them first.");
        }

        categories.Remove(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.CategoryDeleted(id);
    }
}
