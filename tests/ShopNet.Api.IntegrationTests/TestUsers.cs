using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using ShopNet.Contracts.Auth;

namespace ShopNet.Api.IntegrationTests;

/// <summary>Registers and signs in users through the public API. Names are unique so tests sharing the database never collide.</summary>
internal static class TestUsers
{
    public const string Password = "Test-password-1";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static string UniqueName(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    public static async Task<HttpClient> CreateCustomerClientAsync(this WebApplicationFactory<Program> factory, string prefix = "customer")
    {
        var userName = UniqueName(prefix);
        var client = factory.CreateClient();

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest { UserName = userName, Password = Password }, Ct);
        register.StatusCode.ShouldBe(HttpStatusCode.Created);

        await client.SignInAsync(userName, Password);
        return client;
    }

    public static async Task<HttpClient> CreateAdminClientAsync(this WebApplicationFactory<Program> factory)
    {
        var client = factory.CreateClient();
        await client.SignInAsync(ApiFactory.AdminUserName, ApiFactory.AdminPassword);
        return client;
    }

    public static async Task<LoginResponse> SignInAsync(this HttpClient client, string userName, string password)
    {
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { UserName = userName, Password = password }, Ct);
        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        var session = await login.Content.ReadFromJsonAsync<LoginResponse>(Ct);
        session.ShouldNotBeNull();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", session.Token);
        return session;
    }
}
