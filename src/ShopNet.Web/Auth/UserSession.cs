using System.Buffers.Text;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace ShopNet.Web.Auth;

/// <summary>
/// The signed-in user, read from the JWT payload. The client cannot verify the signature (the key is the API's secret),
/// so these claims only drive the UI; the API validates the token on every request.
/// </summary>
public sealed record UserSession(string Token, int UserId, string UserName, string Role, DateTimeOffset ExpiresAt)
{
    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;

    public ClaimsPrincipal ToPrincipal() =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, UserId.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, UserName),
                new Claim(ClaimTypes.Role, Role),
            ],
            authenticationType: "jwt"));

    public static UserSession? FromToken(string? token)
    {
        var parts = token?.Split('.');
        if (parts is not [_, var payload, _])
        {
            return null;
        }

        try
        {
            using var json = JsonDocument.Parse(Base64Url.DecodeFromChars(payload));
            var claims = json.RootElement;

            // A user with several roles would get an array; ShopNet users have exactly one.
            var role = claims.GetProperty("role") is { ValueKind: JsonValueKind.Array } roles
                ? roles[0].GetString()
                : claims.GetProperty("role").GetString();

            return new UserSession(
                token!,
                int.Parse(claims.GetProperty("sub").GetString()!, CultureInfo.InvariantCulture),
                claims.GetProperty("unique_name").GetString()!,
                role!,
                DateTimeOffset.FromUnixTimeSeconds(claims.GetProperty("exp").GetInt64()));
        }
        catch (Exception e) when (e is FormatException or JsonException or KeyNotFoundException or InvalidOperationException
                                      or IndexOutOfRangeException or ArgumentNullException or OverflowException)
        {
            return null;
        }
    }
}
