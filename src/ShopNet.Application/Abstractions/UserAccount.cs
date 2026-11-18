namespace ShopNet.Application.Abstractions;

/// <summary>A user as the application sees it. Credentials stay inside the identity adapter.</summary>
public sealed record UserAccount(int Id, string UserName, string Role);
