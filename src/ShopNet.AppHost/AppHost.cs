var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithImageTag("2025-latest")
    .WithDataVolume("shopnet-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("shopnet");

// Secret parameter: Aspire asks for it in the dashboard the first time, or reads Parameters:admin-password from user-secrets.
var adminPassword = builder.AddParameter("admin-password", secret: true);

var api = builder.AddProject<Projects.ShopNet_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Database__MigrateOnStartup", "true")
    .WithEnvironment("Seed__Admin__UserName", "admin")
    .WithEnvironment("Seed__Admin__Password", adminPassword)
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.ShopNet_Web>("web")
    .WaitFor(api);

await builder.Build().RunAsync();
