using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Products;
using ShopNet.Domain.Categories;
using ShopNet.Domain.Products;

namespace ShopNet.Application.Features.Products;

public sealed class GetProductsHandler(IProductRepository products)
{
    /// <param name="includeInactive">Also return deactivated products (admins only; the endpoint decides).</param>
    /// <param name="categoryId">Only products of this category, when set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<IReadOnlyList<ProductResponse>> HandleAsync(
        bool includeInactive, int? categoryId, CancellationToken cancellationToken)
    {
        var items = await products.ListAsync(includeInactive, categoryId, cancellationToken);
        return [.. items.Select(p => p.ToResponse())];
    }
}

public sealed class GetProductByIdHandler(IProductRepository products)
{
    public async Task<ProductResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);
        return product.ToResponse();
    }
}

public sealed class CreateProductHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork,
    ILogger<CreateProductHandler> logger)
{
    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        var product = Product.Create(request.Name, request.Description, request.Price, category);
        if (!request.IsActive)
        {
            product.Deactivate();
        }

        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductCreated(product.Id);
        return product.ToResponse();
    }
}

public sealed class UpdateProductHandler(
    IProductRepository products,
    ICategoryRepository categories,
    IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        var category = await categories.GetByIdAsync(request.CategoryId, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.CategoryId);

        product.Update(request.Name, request.Description, request.Price, category);
        if (request.IsActive)
        {
            product.Activate();
        }
        else
        {
            product.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Deletes a product that was never ordered. A product with orders is deactivated instead, so past orders keep
/// pointing at it and it disappears from the catalog.
/// </summary>
public sealed class DeleteProductHandler(
    IProductRepository products,
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProductHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        if (await orders.AnyForProductAsync(id, cancellationToken))
        {
            product.Deactivate();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.ProductDeactivated(id);
            return;
        }

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.ProductDeleted(id);
    }
}
