namespace ShopNet.Application.Abstractions;

/// <summary>Outcome of checking a user name and password. <see cref="User"/> is set only when it succeeded.</summary>
public sealed record CredentialCheckResult(CredentialCheckStatus Status, UserAccount? User = null);
