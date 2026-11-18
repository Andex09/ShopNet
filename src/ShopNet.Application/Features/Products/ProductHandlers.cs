using Microsoft.Extensions.Logging;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Products;
using ShopNet.Domain.Products;

namespace ShopNet.Application.Features.Products;

public sealed class GetProductsHandler(IProductRepository products)
{
    public async Task<IReadOnlyList<ProductResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await products.ListAsync(cancellationToken);
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
    IUnitOfWork unitOfWork,
    ILogger<CreateProductHandler> logger)
{
    public async Task<ProductResponse> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = Product.Create(request.Name, request.Price);
        products.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.ProductCreated(product.Id);
        return product.ToResponse();
    }
}

public sealed class UpdateProductHandler(IProductRepository products, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await products.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), id);

        product.Update(request.Name, request.Price);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

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

        // Orders keep their history: a product that was ordered cannot be deleted.
        if (await orders.AnyForProductAsync(id, cancellationToken))
        {
            throw new ConflictException("The product has orders and cannot be deleted.");
        }

        products.Remove(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.ProductDeleted(id);
    }
}
