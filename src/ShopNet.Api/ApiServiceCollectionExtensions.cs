using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using ShopNet.Api.Infrastructure;
using ShopNet.Application.Abstractions;
using ShopNet.Infrastructure.Security;

namespace ShopNet.Api;

public static class ApiServiceCollectionExtensions
{
    public const string CorsPolicyName = "Web";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        // Errors: RFC 9457 problem details everywhere, with the trace id for correlation. Never exception messages.
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        AddAuth(services);
        AddCors(services, configuration);
        services.AddAuthRateLimiting(configuration);
        AddOpenApi(services);

        return services;
    }

    private static void AddAuth(IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured from JwtOptions, which are validated at startup.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwt) => bearer.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(jwt.Value.GetSigningKeyBytes()),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidateIssuer = true,
                ValidIssuer = jwt.Value.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Value.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromSeconds(30),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Admin, policy => policy.RequireRole(Domain.Users.Roles.Admin));
    }

    private static void AddCors(IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));
    }

    private static void AddOpenApi(IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Info.Title = "ShopNet API";
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT from POST /api/auth/login",
            };
            document.Security =
            [
                new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", document)] = [] },
            ];
            return Task.CompletedTask;
        }));
}

public static class Policies
{
    public const string Admin = "Admin";
}
