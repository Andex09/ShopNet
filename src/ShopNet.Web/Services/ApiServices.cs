using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Categories;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;
using ShopNet.Contracts.Users;

namespace ShopNet.Web.Services;

public sealed class AuthService(ApiClient api)
{
    public Task<ApiResult<LoginResponse>> LoginAsync(LoginRequest request) =>
        api.PostAsync<LoginRequest, LoginResponse>("api/auth/login", request);

    public Task<ApiResult<UserResponse>> RegisterAsync(RegisterRequest request) =>
        api.PostAsync<RegisterRequest, UserResponse>("api/auth/register", request);
}

public sealed class CategoryService(ApiClient api)
{
    public Task<ApiResult<List<CategoryResponse>>> GetAllAsync() => api.GetAsync<List<CategoryResponse>>("api/categories");
}

public sealed class ProductService(ApiClient api)
{
    /// <param name="categoryId">Only this category, when set.</param>
    /// <param name="includeInactive">Also deactivated products. The API honours it only for admins.</param>
    public Task<ApiResult<List<ProductResponse>>> GetAllAsync(int? categoryId = null, bool includeInactive = false) =>
        api.GetAsync<List<ProductResponse>>(
            $"api/products?includeInactive={(includeInactive ? "true" : "false")}" + (categoryId is { } id ? $"&categoryId={id}" : string.Empty));

    public Task<ApiResult<ProductResponse>> GetAsync(int id) => api.GetAsync<ProductResponse>($"api/products/{id}");

    public Task<ApiResult<ProductResponse>> CreateAsync(ProductRequest request) =>
        api.PostAsync<ProductRequest, ProductResponse>("api/products", request);

    public Task<ApiResult> UpdateAsync(int id, ProductRequest request) => api.PutAsync($"api/products/{id}", request);

    public Task<ApiResult> DeleteAsync(int id) => api.DeleteAsync($"api/products/{id}");
}

public sealed class UserService(ApiClient api)
{
    public Task<ApiResult<List<UserResponse>>> GetAllAsync() => api.GetAsync<List<UserResponse>>("api/users");

    public Task<ApiResult<UserResponse>> GetAsync(int id) => api.GetAsync<UserResponse>($"api/users/{id}");

    public Task<ApiResult<UserResponse>> CreateAsync(CreateUserRequest request) =>
        api.PostAsync<CreateUserRequest, UserResponse>("api/users", request);

    public Task<ApiResult> UpdateAsync(int id, UpdateUserRequest request) => api.PutAsync($"api/users/{id}", request);

    public Task<ApiResult> DeleteAsync(int id) => api.DeleteAsync($"api/users/{id}");
}

public sealed class OrderService(ApiClient api)
{
    public Task<ApiResult<List<OrderResponse>>> GetAllAsync() => api.GetAsync<List<OrderResponse>>("api/orders");

    public Task<ApiResult<OrderResponse>> GetAsync(int id) => api.GetAsync<OrderResponse>($"api/orders/{id}");

    public Task<ApiResult<OrderResponse>> CreateAsync(CreateOrderRequest request) =>
        api.PostAsync<CreateOrderRequest, OrderResponse>("api/orders", request);

    public Task<ApiResult> UpdateAsync(int id, UpdateOrderRequest request) => api.PutAsync($"api/orders/{id}", request);

    public Task<ApiResult> ConfirmAsync(int id) => api.PostAsync($"api/orders/{id}/confirm");

    public Task<ApiResult> CancelAsync(int id) => api.PostAsync($"api/orders/{id}/cancel");

    public Task<ApiResult<List<OrderResponse>>> GetMineAsync() => api.GetAsync<List<OrderResponse>>("api/me/orders");

    public Task<ApiResult<OrderResponse>> GetMineAsync(int id) => api.GetAsync<OrderResponse>($"api/me/orders/{id}");

    public Task<ApiResult<OrderResponse>> CreateMineAsync(CreateMyOrderRequest request) =>
        api.PostAsync<CreateMyOrderRequest, OrderResponse>("api/me/orders", request);

    public Task<ApiResult> CancelMineAsync(int id) => api.PostAsync($"api/me/orders/{id}/cancel");
}

/// <summary>Order totals shown while filling a form. Only an estimate: the API calculates the real amounts.</summary>
public static class OrderEstimate
{
    public const decimal TaxRate = 0.19m;

    /// <summary>Same rules as the server: each line rounded, tax once on the subtotal.</summary>
    public static decimal LineTotal(decimal unitPrice, int quantity) =>
        Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);

    public static (decimal Subtotal, decimal Tax, decimal Total) Calculate(IEnumerable<(decimal UnitPrice, int Quantity)> lines)
    {
        var subtotal = lines.Sum(l => LineTotal(l.UnitPrice, l.Quantity));
        var tax = Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        return (subtotal, tax, subtotal + tax);
    }
}
