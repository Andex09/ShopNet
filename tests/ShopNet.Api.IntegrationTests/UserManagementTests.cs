using System.Net;
using System.Net.Http.Json;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Users;

namespace ShopNet.Api.IntegrationTests;

[Collection(SharedApi.Name)]
public class UserManagementTests(ApiFactory factory)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task The_admin_creates_a_user_who_can_then_sign_in()
    {
        var admin = await factory.CreateAdminClientAsync();
        var userName = TestUsers.UniqueName("created");

        var user = await CreateAsync(admin, userName, RoleNames.Admin);

        user.UserName.ShouldBe(userName);
        user.Role.ShouldBe(RoleNames.Admin);
        (await admin.GetFromJsonAsync<UserResponse>($"/api/users/{user.Id}", Ct)).ShouldBe(user);
        var session = await factory.CreateClient().SignInAsync(userName, TestUsers.Password);
        session.Token.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task User_names_are_unique()
    {
        var admin = await factory.CreateAdminClientAsync();
        var userName = TestUsers.UniqueName("taken");
        await CreateAsync(admin, userName, RoleNames.Customer);

        var response = await admin.PostAsJsonAsync("/api/users", Request(userName, RoleNames.Customer), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task An_unknown_role_is_a_validation_problem()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/users", Request(TestUsers.UniqueName("role"), "SuperUser"), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Updating_a_user_changes_the_name_and_role_and_keeps_the_password_unless_one_is_given()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await CreateAsync(admin, TestUsers.UniqueName("before"), RoleNames.Customer);
        var renamed = TestUsers.UniqueName("after");

        (await admin.PutAsJsonAsync($"/api/users/{user.Id}",
            new UpdateUserRequest { UserName = renamed, Role = RoleNames.Admin }, Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await admin.GetFromJsonAsync<UserResponse>($"/api/users/{user.Id}", Ct)).ShouldBe(new UserResponse(user.Id, renamed, RoleNames.Admin));
        await factory.CreateClient().SignInAsync(renamed, TestUsers.Password);

        (await admin.PutAsJsonAsync($"/api/users/{user.Id}",
            new UpdateUserRequest { UserName = renamed, Role = RoleNames.Admin, Password = "New-password-2" }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var oldPassword = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new LoginRequest { UserName = renamed, Password = TestUsers.Password }, Ct);
        oldPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        await factory.CreateClient().SignInAsync(renamed, "New-password-2");
    }

    [Fact]
    public async Task A_user_without_orders_can_be_deleted()
    {
        var admin = await factory.CreateAdminClientAsync();
        var user = await CreateAsync(admin, TestUsers.UniqueName("gone"), RoleNames.Customer);

        (await admin.DeleteAsync($"/api/users/{user.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await admin.GetAsync($"/api/users/{user.Id}", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_user_with_orders_cannot_be_deleted()
    {
        var admin = await factory.CreateAdminClientAsync();
        var customer = await factory.CreateCustomerClientAsync();
        var created = await customer.PostAsJsonAsync("/api/me/orders", TestOrders.Of(2, 1), Ct);
        var userId = (await created.Content.ReadFromJsonAsync<Contracts.Orders.OrderResponse>(Ct)).ShouldNotBeNull().UserId;

        var response = await admin.DeleteAsync($"/api/users/{userId}", Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task A_missing_user_is_not_found()
    {
        var admin = await factory.CreateAdminClientAsync();

        (await admin.GetAsync("/api/users/999999", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.PutAsJsonAsync("/api/users/999999",
            new UpdateUserRequest { UserName = TestUsers.UniqueName("ghost"), Role = RoleNames.Customer }, Ct))
            .StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await admin.DeleteAsync("/api/users/999999", Ct)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static CreateUserRequest Request(string userName, string role) =>
        new() { UserName = userName, Password = TestUsers.Password, Role = role };

    private static async Task<UserResponse> CreateAsync(HttpClient admin, string userName, string role)
    {
        var response = await admin.PostAsJsonAsync("/api/users", Request(userName, role), Ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<UserResponse>(Ct)).ShouldNotBeNull();
    }
}
