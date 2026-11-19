using System.Security.Claims;
using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Products;
using ShopNet.Contracts.Products;
using ShopNet.Domain.Users;

namespace ShopNet.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products").RequireAuthorization();

        group.MapGet("/", async (int? categoryId, bool? includeInactive, ClaimsPrincipal user, GetProductsHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(
                    includeInactive: includeInactive == true && user.IsInRole(Roles.Admin), categoryId, ct)))
            .WithName("GetProducts")
            .WithSummary("Active products, optionally by category. Admins can add includeInactive=true.");

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
            .WithSummary("Deletes a product that was never ordered; one with orders is deactivated instead.")
            .RequireAuthorization(Policies.Admin)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
