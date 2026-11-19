using System.Net;
using System.Net.Http.Headers;

namespace ShopNet.Web.Auth;

/// <summary>
/// Adds the JWT to every API request. When the API answers 401 to a request that carried a token, the token is no
/// longer valid (expired, or the signing key changed), so the session ends and the UI shows the login form.
/// </summary>
public sealed class BearerTokenHandler(SessionAuthStateProvider session) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var token = await session.GetTokenAsync();
        if (token is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (token is not null && response.StatusCode == HttpStatusCode.Unauthorized)
        {
            await session.SignOutAsync(expired: true);
        }

        return response;
    }
}
