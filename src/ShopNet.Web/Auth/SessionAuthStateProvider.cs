using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ShopNet.Web.Auth;

/// <summary>
/// Keeps the JWT in session storage and exposes its claims as the authentication state.
/// The session ends when the token expires (timer) or when the API rejects it (see <see cref="BearerTokenHandler"/>).
/// </summary>
public sealed class SessionAuthStateProvider(ISessionStorageService sessionStorage, TimeProvider timeProvider)
    : AuthenticationStateProvider, IDisposable
{
    private const string StorageKey = "shopnet.token";
    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    private UserSession? _session;
    private bool _loaded;
    private ITimer? _expiryTimer;

    /// <summary>True when the last session ended because the token expired or was rejected. Cleared on sign-in.</summary>
    public bool SessionExpired { get; private set; }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await GetSessionAsync();
        return session is null ? Anonymous : new AuthenticationState(session.ToPrincipal());
    }

    public async Task<string?> GetTokenAsync() => (await GetSessionAsync())?.Token;

    public async Task SignInAsync(string token)
    {
        var session = UserSession.FromToken(token)
            ?? throw new InvalidOperationException("The API returned a token without the expected claims.");

        await sessionStorage.SetItemAsStringAsync(StorageKey, token);
        SessionExpired = false;
        Start(session);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(session.ToPrincipal())));
    }

    /// <param name="expired">True when the session ends because the token is no longer valid, not by the user's choice.</param>
    public async Task SignOutAsync(bool expired = false)
    {
        var wasSignedIn = _session is not null;
        Stop();
        await sessionStorage.RemoveItemAsync(StorageKey);

        SessionExpired = expired && wasSignedIn;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public void Dispose() => _expiryTimer?.Dispose();

    private async Task<UserSession?> GetSessionAsync()
    {
        if (!_loaded)
        {
            _loaded = true;
            var session = UserSession.FromToken(await sessionStorage.GetItemAsStringAsync(StorageKey));
            if (session is not null && !session.IsExpired(timeProvider.GetUtcNow()))
            {
                Start(session);
            }
            else
            {
                await sessionStorage.RemoveItemAsync(StorageKey);
            }
        }

        if (_session is not null && _session.IsExpired(timeProvider.GetUtcNow()))
        {
            await SignOutAsync(expired: true);
        }

        return _session;
    }

    private void Start(UserSession session)
    {
        Stop();
        _session = session;

        var remaining = session.ExpiresAt - timeProvider.GetUtcNow();
        _expiryTimer = timeProvider.CreateTimer(
            _ => _ = SignOutAsync(expired: true),
            state: null,
            dueTime: remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero,
            period: Timeout.InfiniteTimeSpan);
    }

    private void Stop()
    {
        _expiryTimer?.Dispose();
        _expiryTimer = null;
        _session = null;
    }
}
