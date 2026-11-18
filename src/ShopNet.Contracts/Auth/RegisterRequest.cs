using System.ComponentModel.DataAnnotations;

namespace ShopNet.Contracts.Auth;

public sealed class RegisterRequest
{
    [Required, StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}
