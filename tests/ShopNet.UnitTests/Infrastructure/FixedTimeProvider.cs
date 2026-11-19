namespace ShopNet.UnitTests.Infrastructure;

/// <summary>A clock that always returns the same instant, so token times are predictable.</summary>
internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => now;
}
