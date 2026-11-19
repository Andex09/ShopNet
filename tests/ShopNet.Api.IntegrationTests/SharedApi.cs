namespace ShopNet.Api.IntegrationTests;

/// <summary>All integration test classes share one API and one SQL Server container (starting one takes ~10 s).</summary>
[CollectionDefinition(Name)]
public sealed class SharedApi : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
