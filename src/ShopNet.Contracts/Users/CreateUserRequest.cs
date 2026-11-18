using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Users;

public sealed class CreateUserRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = RoleNames.Basic;
}
