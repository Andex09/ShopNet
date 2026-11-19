using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Orders;
using ShopNet.Contracts.Orders;

namespace ShopNet.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
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
            .WithSummary("Create a pending order for a user.")
            .WithValidation<CreateOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        admin.MapPut("/{id:int}", async (int id, UpdateOrderRequest request, UpdateOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, request, ct);
                return TypedResults.NoContent();
            })
            .WithName("UpdateOrder")
            .WithSummary("Replace the lines of a pending order.")
            .WithValidation<UpdateOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPost("/{id:int}/confirm", async (int id, ConfirmOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("ConfirmOrder")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        admin.MapPost("/{id:int}/cancel", async (int id, CancelOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("CancelOrder")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        // Signed-in user: only their own orders. The user id comes from the token; other users' orders are 404.
        var mine = app.MapGroup("/api/me/orders").WithTags("My orders").RequireAuthorization();

        mine.MapGet("/", async (GetMyOrdersHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetMyOrders");

        mine.MapGet("/{id:int}", async (int id, GetMyOrderByIdHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(id, ct)))
            .WithName("GetMyOrder")
            .ProducesProblem(StatusCodes.Status404NotFound);

        mine.MapPost("/", async (CreateMyOrderRequest request, CreateMyOrderHandler handler, CancellationToken ct) =>
            {
                var order = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/me/orders/{order.Id}", order);
            })
            .WithName("CreateMyOrder")
            .WithSummary("Place a pending order.")
            .WithValidation<CreateMyOrderRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        mine.MapPost("/{id:int}/cancel", async (int id, CancelMyOrderHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("CancelMyOrder")
            .WithSummary("Cancel one of my pending orders.")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
