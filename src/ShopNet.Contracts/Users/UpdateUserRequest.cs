using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Users;

public sealed class UpdateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Optional. The password is only changed when a value is provided.</summary>
    [StringLength(100, MinimumLength = 8)]
    public string? Password { get; set; }

    [Required]
    public string Role { get; set; } = RoleNames.Customer;
}
