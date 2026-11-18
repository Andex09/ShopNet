using ShopNet.Api.Infrastructure;
using ShopNet.Application.Features.Auth;
using ShopNet.Contracts.Auth;

namespace ShopNet.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth").AllowAnonymous();

        group.MapPost("/register", async (RegisterRequest request, RegisterHandler handler, CancellationToken ct) =>
            {
                var user = await handler.HandleAsync(request, ct);
                return TypedResults.Created($"/api/users/{user.Id}", user);
            })
            .WithName("Register")
            .WithSummary("Create an account with the Customer role.")
            .WithValidation<RegisterRequest>()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", async (LoginRequest request, LoginHandler handler, CancellationToken ct) =>
                await handler.HandleAsync(request, ct) is { } response
                    ? Results.Ok(response)
                    : Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid user name or password"))
            .WithName("Login")
            .WithSummary("Exchange credentials for a JWT.")
            .WithValidation<LoginRequest>()
            .Produces<LoginResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
