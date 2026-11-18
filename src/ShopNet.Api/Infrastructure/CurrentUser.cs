using System.Globalization;
using System.Security.Claims;
using ShopNet.Application.Abstractions;

namespace ShopNet.Api.Infrastructure;

internal sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public int? UserId =>
        int.TryParse(
            accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out var id)
            ? id
            : null;
}
