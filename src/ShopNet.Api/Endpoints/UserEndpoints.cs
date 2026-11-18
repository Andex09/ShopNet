using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Users;
using ShopNet.Contracts.Users;

namespace ShopNet.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users").RequireAuthorization(Policies.Admin);

        group.MapGet("/", async (GetUsersHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(ct)))
            .WithName("GetUsers");

        group.MapGet("/{id:int}", async (int id, GetUserByIdHandler handler, CancellationToken ct) =>
                TypedResults.Ok(await handler.HandleAsync(id, ct)))
            .WithName("GetUser")
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateUserRequest request, CreateUserHandler handler, CancellationToken ct) =>
            {
                var user = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/users/{user.Id}", user);
            })
            .WithName("CreateUser")
            .WithValidation<CreateUserRequest>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:int}", async (int id, UpdateUserRequest request, UpdateUserHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, request, ct);
                return TypedResults.NoContent();
            })
            .WithName("UpdateUser")
            .WithValidation<UpdateUserRequest>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:int}", async (int id, DeleteUserHandler handler, CancellationToken ct) =>
            {
                await handler.HandleAsync(id, ct);
                return TypedResults.NoContent();
            })
            .WithName("DeleteUser")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
