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

    private static async Task<ProductResponse> CreateProductAsync(HttpClient admin, string name, bool isActive = true)
    {
        var response = await admin.PostAsJsonAsync("/api/products",
            new ProductRequest { Name = name, Description = "Creado por una prueba.", Price = 10_000m, CategoryId = 8, IsActive = isActive }, Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<ProductResponse>(Ct)).ShouldNotBeNull();
    }
}
