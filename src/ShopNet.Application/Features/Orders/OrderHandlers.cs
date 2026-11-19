using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Contracts.Orders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;

namespace ShopNet.Application.Features.Orders;

public sealed class GetOrdersHandler(IOrderRepository orders)
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var items = await orders.ListAsync(cancellationToken);
        return [.. items.Select(o => o.ToResponse())];
    }
}

public sealed class GetOrderByIdHandler(IOrderRepository orders)
{
    public async Task<OrderResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
        return order.ToResponse();
    }
}

/// <summary>Orders of the signed-in user. The user id comes from the token, never from the URL.</summary>
public sealed class GetMyOrdersHandler(IOrderRepository orders, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<OrderResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");
        var items = await orders.ListByUserAsync(userId, cancellationToken);
        return [.. items.Select(o => o.ToResponse())];
    }
}

/// <summary>Creates an order. Prices and totals are calculated from the stored product, not from the request.</summary>
public sealed class CreateOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUserAccounts users,
    IUnitOfWork unitOfWork,
    IOptions<OrderOptions> options,
    ILogger<CreateOrderHandler> logger)
{
    public async Task<OrderResponse> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await users.ExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        var product = await products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var order = Order.Create(request.UserId, product, request.Quantity, options.Value.TaxRate);
        orders.Add(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.OrderCreated(order.Id, order.UserId, order.Total);
        return order.ToResponse();
    }
}

public sealed class CreateMyOrderHandler(CreateOrderHandler createOrder, ICurrentUser currentUser)
{
    public Task<OrderResponse> HandleAsync(CreateMyOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");
        return createOrder.HandleAsync(
            new CreateOrderRequest { UserId = userId, ProductId = request.ProductId, Quantity = request.Quantity },
            cancellationToken);
    }
}

public sealed class UpdateOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUnitOfWork unitOfWork,
    IOptions<OrderOptions> options)
{
    public async Task HandleAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        var product = await products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        order.Change(product, request.Quantity, options.Value.TaxRate);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class DeleteOrderHandler(
    IOrderRepository orders,
    IUnitOfWork unitOfWork,
    ILogger<DeleteOrderHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        orders.Remove(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.OrderDeleted(id);
    }
}
