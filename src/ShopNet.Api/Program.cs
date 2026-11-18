using Scalar.AspNetCore;
using ShopNet.Api;
using ShopNet.Api.Endpoints;
using ShopNet.Application;
using ShopNet.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddInfrastructure();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddApi(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer"));
}

if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    await app.Services.MigrateDatabaseAsync();
}

await app.Services.SeedAdminAsync();

app.UseHttpsRedirection();
app.UseCors(ApiServiceCollectionExtensions.CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapUserEndpoints();
app.MapOrderEndpoints();

await app.RunAsync();

/// <summary>Entry point, public so integration tests can use WebApplicationFactory&lt;Program&gt;.</summary>
public partial class Program;
