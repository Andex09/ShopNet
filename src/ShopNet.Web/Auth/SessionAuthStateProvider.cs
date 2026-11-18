using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using ShopNet.Contracts.Auth;

namespace ShopNet.Web.Auth;

public sealed class SessionAuthStateProvider(ISessionStorageService sessionStorage, HttpClient httpClient)
    : AuthenticationStateProvider
{
    private const string StorageKey = "shopnet.session";
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await sessionStorage.GetItemAsync<LoginResponse>(StorageKey);
        if (session is null)
        {
            return Anonymous;
        }

        SetBearerToken(session.Token);
        return new AuthenticationState(CreatePrincipal(session));
    }

    public async Task SignInAsync(LoginResponse session)
    {
        ArgumentNullException.ThrowIfNull(session);

        await sessionStorage.SetItemAsync(StorageKey, session);
        SetBearerToken(session.Token);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(session))));
    }

    public async Task SignOutAsync()
    {
        await sessionStorage.RemoveItemAsync(StorageKey);
        httpClient.DefaultRequestHeaders.Authorization = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    private void SetBearerToken(string token) =>
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    private static ClaimsPrincipal CreatePrincipal(LoginResponse session) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, session.User.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, session.User.UserName),
                new Claim(ClaimTypes.Role, session.User.Role),
            ],
            authenticationType: "jwt"));
}
