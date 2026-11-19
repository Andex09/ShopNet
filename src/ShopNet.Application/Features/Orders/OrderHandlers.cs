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
        var items = await orders.ListByUserAsync(currentUser.RequireUserId(), cancellationToken);
        return [.. items.Select(o => o.ToResponse())];
    }
}

/// <summary>One of the signed-in user's orders. Someone else's order is reported as not found, not as forbidden.</summary>
public sealed class GetMyOrderByIdHandler(IOrderRepository orders, ICurrentUser currentUser)
{
    public async Task<OrderResponse> HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOwnedAsync(id, currentUser, cancellationToken);
        return order.ToResponse();
    }
}

/// <summary>Creates a pending order. Prices and totals are calculated from the stored products, not from the request.</summary>
public sealed class CreateOrderHandler(
    IOrderRepository orders,
    IProductRepository products,
    IUserAccounts users,
    IUnitOfWork unitOfWork,
    IOptions<OrderOptions> options,
    TimeProvider timeProvider,
    ILogger<CreateOrderHandler> logger)
{
    public async Task<OrderResponse> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await users.ExistsAsync(request.UserId, cancellationToken))
        {
            throw new NotFoundException("User", request.UserId);
        }

        var lines = await products.LoadLinesAsync(request.Lines, cancellationToken);
        var order = Order.Create(request.UserId, lines, options.Value.TaxRate, timeProvider.GetUtcNow());
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

        return createOrder.HandleAsync(
            new CreateOrderRequest { UserId = currentUser.RequireUserId(), Lines = request.Lines },
            cancellationToken);
    }
}

public sealed class UpdateOrderHandler(IOrderRepository orders, IProductRepository products, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, UpdateOrderRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.ReplaceLines(await products.LoadLinesAsync(request.Lines, cancellationToken));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ConfirmOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork, ILogger<ConfirmOrderHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Confirm();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.OrderConfirmed(id);
    }
}

public sealed class CancelOrderHandler(IOrderRepository orders, IUnitOfWork unitOfWork, ILogger<CancelOrderHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);

        order.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.OrderCancelled(id);
    }
}

public sealed class CancelMyOrderHandler(
    IOrderRepository orders,
    ICurrentUser currentUser,
    IUnitOfWork unitOfWork,
    ILogger<CancelMyOrderHandler> logger)
{
    public async Task HandleAsync(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOwnedAsync(id, currentUser, cancellationToken);

        order.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.OrderCancelled(id);
    }
}

internal static class OrderHandlerExtensions
{
    public static int RequireUserId(this ICurrentUser currentUser) =>
        currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

    public static async Task<Order> GetOwnedAsync(
        this IOrderRepository orders, int id, ICurrentUser currentUser, CancellationToken cancellationToken)
    {
        var order = await orders.GetByIdAsync(id, cancellationToken);
        return order is not null && order.UserId == currentUser.RequireUserId()
            ? order
            : throw new NotFoundException(nameof(Order), id);
    }

    /// <summary>Loads every requested product in one query. An unknown product is a 404 naming it.</summary>
    public static async Task<IReadOnlyList<(Product Product, int Quantity)>> LoadLinesAsync(
        this IProductRepository products, IReadOnlyCollection<OrderLineRequest> lines, CancellationToken cancellationToken)
    {
        var ids = lines.Select(l => l.ProductId).Distinct().ToList();
        var found = (await products.GetByIdsAsync(ids, cancellationToken)).ToDictionary(p => p.Id);

        return [.. lines.Select(l => found.TryGetValue(l.ProductId, out var product)
            ? (product, l.Quantity)
            : throw new NotFoundException(nameof(Product), l.ProductId))];
    }
}
