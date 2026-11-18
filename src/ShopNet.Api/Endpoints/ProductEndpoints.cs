using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Products;
using ShopNet.Contracts.Products;

namespace ShopNet.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", async (GetProductsHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetProducts");

        group.MapGet("/{id:int}", async (int id, GetProductByIdHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(id, ct)))
            .WithName("GetProduct")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (ProductRequest request, CreateProductHandler handler, CancellationToken ct) =>
            {
                var product = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/products/{product.Id}", product);
            })
            .WithName("CreateProduct")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<ProductRequest>();

        group.MapPut("/{id:int}", async (int id, ProductRequest request, UpdateProductHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, request, ct);
                return TypedResults.NoContent();
            })
            .WithName("UpdateProduct")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<ProductRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, DeleteProductHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("DeleteProduct")
            .RequireAuthorization(Policies.Admin)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
