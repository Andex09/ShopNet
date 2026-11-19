using Microsoft.AspNetCore.Identity;

namespace ShopNet.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity user. The key stays an <c>int</c>, so <c>Orders.UserId</c> keeps pointing at it.</summary>
public sealed class AppUser : IdentityUser<int>;
