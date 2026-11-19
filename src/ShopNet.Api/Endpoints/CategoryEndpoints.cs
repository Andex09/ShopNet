using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Categories;
using ShopNet.Contracts.Categories;

namespace ShopNet.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").WithTags("Categories").RequireAuthorization();

        group.MapGet("/", async (GetCategoriesHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetCategories");

        group.MapGet("/{id:int}", async (int id, GetCategoryByIdHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(id, ct)))
            .WithName("GetCategory")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CategoryRequest request, CreateCategoryHandler handler, CancellationToken ct) =>
            {
                var category = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/categories/{category.Id}", category);
            })
            .WithName("CreateCategory")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<CategoryRequest>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, CategoryRequest request, UpdateCategoryHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, request, ct);
                return TypedResults.NoContent();
            })
            .WithName("UpdateCategory")
            .RequireAuthorization(Policies.Admin)
            .WithValidation<CategoryRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, DeleteCategoryHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("DeleteCategory")
            .RequireAuthorization(Policies.Admin)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
