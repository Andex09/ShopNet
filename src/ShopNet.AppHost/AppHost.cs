var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithImageTag("2025-latest")
    .WithDataVolume("shopnet-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("shopnet");

var api = builder.AddProject<Projects.ShopNet_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Database__MigrateOnStartup", "true")
    .WithHttpHealthCheck("/health/ready");

builder.AddProject<Projects.ShopNet_Web>("web")
    .WaitFor(api);

await builder.Build().RunAsync();
