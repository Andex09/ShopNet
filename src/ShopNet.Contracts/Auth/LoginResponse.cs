using ShopNet.Contracts.Users;

namespace ShopNet.Contracts.Auth;

public sealed record LoginResponse(string Token, UserResponse User);
