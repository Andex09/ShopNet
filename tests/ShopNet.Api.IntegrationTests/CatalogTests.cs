using System.Net;
using System.Net.Http.Json;
using ShopNet.Contracts.Categories;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class CatalogTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_migration_seeds_a_technology_catalog_in_eight_categories()
    {
        var client = await factory.CreateCustomerClientAsync();

        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories", Ct);
        var products = await client.GetFromJsonAsync<List<ProductResponse>>("/api/products", Ct);

        categories.ShouldNotBeNull().Count.ShouldBeGreaterThanOrEqualTo(8);
        products.ShouldNotBeNull().Count(p => p.CategoryId <= 8).ShouldBeGreaterThanOrEqualTo(60);
        products.ShouldAllBe(p => p.IsActive && p.Description.Length > 0 && p.CategoryName.Length > 0);
        products.ShouldNotContain(p => p.Name == "Platzi");
    }

    [Fact]
    public async Task Products_can_be_filtered_by_category()
    {
        var client = await factory.CreateCustomerClientAsync();

        var audio = await client.GetFromJsonAsync<List<ProductResponse>>("/api/products?categoryId=1", Ct);

        audio.ShouldNotBeNull().ShouldNotBeEmpty();
        audio.ShouldAllBe(p => p.CategoryId == 1 && p.CategoryName == "Audio");
    }

    [Fact]
    public async Task Deleting_an_ordered_product_deactivates_it_instead()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var product = await CreateProductAsync(admin, "Producto con pedidos");
        (await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(product.Id, 1), Ct))
            .StatusCode.ShouldBe(HttpStatusCode.Created);

        (await admin.DeleteAsync($"/api/products/{product.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Still there for order history, hidden from the catalog and no longer orderable.
        var stored = await admin.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}", Ct);
        stored.ShouldNotBeNull().IsActive.ShouldBeFalse();
        (await customer.GetFromJsonAsync<List<ProductResponse>>("/api/products", Ct)).ShouldNotBeNull()
            .ShouldNotContain(p => p.Id == product.Id);
        (await admin.GetFromJsonAsync<List<ProductResponse>>("/api/products?includeInactive=true", Ct)).ShouldNotBeNull()
            .ShouldContain(p => p.Id == product.Id);
        (await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(product.Id, 1), Ct))
            .StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deleting_a_product_that_was_never_ordered_removes_it()
    {
        var admin = await factory.CreateAdminClientAsync();
        var product = await CreateProductAsync(admin, "Producto sin pedidos");

        (await admin.DeleteAsync($"/api/products/{product.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await admin.GetAsync($"/api/products/{product.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customers_cannot_see_inactive_products_even_if_they_ask()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var product = await CreateProductAsync(admin, "Producto oculto", isActive: false);

        var visible = await customer.GetFromJsonAsync<List<ProductResponse>>("/api/products?includeInactive=true", Ct);

        visible.ShouldNotBeNull().ShouldNotContain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task A_category_with_products_cannot_be_deleted()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.DeleteAsync("/api/categories/1", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Categories_can_be_created_renamed_and_deleted_and_names_are_unique()
    {
        var admin = await factory.CreateAdminClientAsync();
        var name = $"Cat {Guid.NewGuid():N}"[..20];

        var created = await admin.PostAsJsonAsync("/api/categories", new CategoryRequest { Name = name }, Ct);
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var category = await created.Content.ReadFromJsonAsync<CategoryResponse>(Ct);
        category.ShouldNotBeNull();
        (await admin.GetFromJsonAsync<CategoryResponse>($"/api/categories/{category.Id}", Ct)).ShouldBe(category);

        (await admin.PostAsJsonAsync("/api/categories", new CategoryRequest { Name = name }, Ct)).StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await admin.PutAsJsonAsync($"/api/categories/{category.Id}", new CategoryRequest { Name = name + " 2" }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.DeleteAsync($"/api/categories/{category.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.GetAsync($"/api/categories/{category.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Creating_a_product_in_a_missing_category_returns_404()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/products",
            new ProductRequest { Name = "Sin categoría", Price = 1_000m, CategoryId = 9999 }, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Updating_a_product_prices_new_orders_but_not_past_ones()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var product = await CreateProductAsync(admin, "Producto que sube de precio");
        var before = await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(product.Id, 1), Ct);
        var pastOrder = (await before.Content.ReadFromJsonAsync<OrderResponse>(Ct)).ShouldNotBeNull();

        (await admin.PutAsJsonAsync($"/api/products/{product.Id}", new ProductRequest
        {
            Name = "Producto actualizado", Description = "Nueva descripción.", Price = 12_500m, CategoryId = 1,
        }, Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updated = await admin.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}", Ct);
        updated.ShouldBe(new ProductResponse(product.Id, "Producto actualizado", "Nueva descripción.", 12_500m, 1, "Audio", true));
        var after = await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(product.Id, 1), Ct);
        (await after.Content.ReadFromJsonAsync<OrderResponse>(Ct)).ShouldNotBeNull().Lines[0].UnitPrice.ShouldBe(12_500m);
        var past = await customer.GetFromJsonAsync<OrderResponse>($"/api/me/orders/{pastOrder.Id}", Ct);
        past.ShouldNotBeNull().Lines[0].ShouldBe(new OrderLineResponse(product.Id, product.Name, 10_000m, 1, 10_000m));
    }

    [Fact]
    public async Task A_missing_product_or_category_is_not_found()
    {
        var admin = await factory.CreateAdminClientAsync();
        var product = new ProductRequest { Name = "Fantasma", Price = 1_000m, CategoryId = 8 };

        (await admin.PutAsJsonAsync("/api/products/999999", product, Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.DeleteAsync("/api/products/999999", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.GetAsync("/api/categories/999999", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.PutAsJsonAsync("/api/categories/999999", new CategoryRequest { Name = "Fantasma" }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.DeleteAsync("/api/categories/999999", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<ProductResponse> CreateProductAsync(HttpClient admin, string name, bool isActive = true)
    {
        var response = await admin.PostAsJsonAsync("/api/products",
            new ProductRequest { Name = name, Description = "Creado por una prueba.", Price = 10_000m, CategoryId = 8, IsActive = isActive }, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>(Ct)).ShouldNotBeNull();
    }
}
