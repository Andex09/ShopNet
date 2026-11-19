using System.Globalization;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;

namespace ShopNet.Web.Services;

/// <summary>
/// The signed-in customer's cart, kept in session storage so it survives navigation and reloads.
/// Stored per user, so whoever signs in next in the same tab does not inherit it.
/// </summary>
public sealed class CartService(ISessionStorageService sessionStorage, AuthenticationStateProvider authState)
{
    public const int MaxQuantity = 1000;

    private List<CartItem> _items = [];
    private string? _loadedKey;

    /// <summary>Raised after any change, so the menu badge and the cart page can refresh.</summary>
    public event Action? Changed;

    public IReadOnlyList<CartItem> Items => _items;

    public int Count => _items.Sum(i => i.Quantity);

    public async Task LoadAsync()
    {
        var key = await StorageKeyAsync();
        if (key == _loadedKey)
        {
            return;
        }

        _loadedKey = key;
        _items = key is null ? [] : await sessionStorage.GetItemAsync<List<CartItem>>(key) ?? [];
        Changed?.Invoke();
    }

    public async Task AddAsync(ProductResponse product, int quantity = 1)
    {
        ArgumentNullException.ThrowIfNull(product);

        await LoadAsync();
        var current = _items.Find(i => i.ProductId == product.Id)?.Quantity ?? 0;
        await SetQuantityAsync(product.Id, product.Name, product.Price, current + quantity);
    }

    public async Task SetQuantityAsync(int productId, int quantity)
    {
        await LoadAsync();
        if (_items.Find(i => i.ProductId == productId) is { } item)
        {
            await SetQuantityAsync(productId, item.ProductName, item.UnitPrice, quantity);
        }
    }

    public async Task RemoveAsync(int productId)
    {
        await LoadAsync();
        await SaveAsync([.. _items.Where(i => i.ProductId != productId)]);
    }

    public async Task ClearAsync()
    {
        await LoadAsync();
        await SaveAsync([]);
    }

    /// <summary>Refreshes names and prices from the catalog. Returns the ids that are no longer available.</summary>
    public async Task<IReadOnlyList<int>> RefreshAsync(IReadOnlyCollection<ProductResponse> catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        await LoadAsync();
        var byId = catalog.ToDictionary(p => p.Id);
        var unavailable = _items.Where(i => !byId.ContainsKey(i.ProductId)).Select(i => i.ProductId).ToList();

        await SaveAsync([.. _items.Select(i => byId.TryGetValue(i.ProductId, out var p)
            ? i with { ProductName = p.Name, UnitPrice = p.Price }
            : i)]);
        return unavailable;
    }

    public CreateMyOrderRequest ToOrderRequest() =>
        new() { Lines = [.. _items.Select(i => new OrderLineRequest { ProductId = i.ProductId, Quantity = i.Quantity })] };

    private Task SetQuantityAsync(int productId, string name, decimal price, int quantity)
    {
        if (quantity <= 0)
        {
            return RemoveAsync(productId);
        }

        var item = new CartItem(productId, name, price, Math.Min(quantity, MaxQuantity));
        var items = _items.ToList();
        var index = items.FindIndex(i => i.ProductId == productId);
        if (index >= 0)
        {
            items[index] = item;
        }
        else
        {
            items.Add(item);
        }

        return SaveAsync(items);
    }

    private async Task SaveAsync(List<CartItem> items)
    {
        _items = items;
        if (_loadedKey is { } key)
        {
            await sessionStorage.SetItemAsync(key, items);
        }

        Changed?.Invoke();
    }

    private async Task<string?> StorageKeyAsync()
    {
        var user = (await authState.GetAuthenticationStateAsync()).User;
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value is { } id
            ? string.Create(CultureInfo.InvariantCulture, $"shopnet.cart.{id}")
            : null;
    }
}
