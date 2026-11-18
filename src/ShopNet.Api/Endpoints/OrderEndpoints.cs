using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Orders;
using ShopNet.Contracts.Orders;

namespace ShopNet.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        // Admin: every order.
        var admin = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization(Policies.Admin);

        admin.MapGet("/", async (GetOrdersHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetOrders");

        admin.MapGet("/{id:int}", async (int id, GetOrderByIdHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(id, ct)))
            .WithName("GetOrder")
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPost("/", async (CreateOrderRequest request, CreateOrderHandler handler, CancellationToken ct) =>
            {
                var order = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/orders/{order.Id}", order);
            })
            .WithName("CreateOrder")
            .WithValidation<CreateOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPut("/{id:int}", async (int id, UpdateOrderRequest request, UpdateOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, request, ct);
                return TypedResults.NoContent();
            })
            .WithName("UpdateOrder")
            .WithValidation<UpdateOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapDelete("/{id:int}", async (int id, DeleteOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("DeleteOrder")
            .ProducesProblem(StatusCodes.Status404NotFound);

        // Signed-in user: only their own orders. The user id comes from the token.
        var mine = app.MapGroup("/api/me/orders").WithTags("My orders").RequireAuthorization();

        mine.MapGet("/", async (GetMyOrdersHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetMyOrders");

        mine.MapPost("/", async (CreateMyOrderRequest request, CreateMyOrderHandler handler, CancellationToken ct) =>
            {
                var order = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/me/orders/{order.Id}", order);
            })
            .WithName("CreateMyOrder")
            .WithValidation<CreateMyOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
