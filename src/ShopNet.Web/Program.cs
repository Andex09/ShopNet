using System.Globalization;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ShopNet.Web;
using ShopNet.Web.Auth;
using ShopNet.Web.Services;

// Colombian pesos and Spanish formatting, regardless of the browser language.
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("es-CO");

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The API URL comes from wwwroot/appsettings.json (overridden per environment at deploy time).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("Missing 'ApiBaseUrl' in wwwroot/appsettings.json.");

builder.Services.AddScoped(sp => new HttpClient(new BearerTokenHandler(sp.GetRequiredService<SessionAuthStateProvider>())
{
    InnerHandler = new HttpClientHandler(),
})
{
    BaseAddress = new Uri(apiBaseUrl),
});
builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<SessionAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SessionAuthStateProvider>());

await builder.Build().RunAsync();
