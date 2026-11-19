namespace ShopNet.Domain.Users;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";

    public static bool IsValid(string? role) => role is Admin or Customer;
}
