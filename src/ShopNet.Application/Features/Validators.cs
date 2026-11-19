using FluentValidation;
using ShopNet.Contracts.Auth;
using ShopNet.Contracts.Categories;
using ShopNet.Contracts.Orders;
using ShopNet.Contracts.Products;
using ShopNet.Contracts.Users;
using ShopNet.Domain.Categories;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Domain.Users;

namespace ShopNet.Application.Features;

// Request validation runs in the API before the handler (endpoint filter) and returns RFC 9457 validation problems.
// The domain entities still guard their own invariants.

internal static class ValidationRules
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 100;
    public const int UserNameMinLength = 3;
    public const int UserNameMaxLength = 50;

    public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Length(UserNameMinLength, UserNameMaxLength)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("User name can only contain letters, digits, '.', '_' and '-'.");

    public static IRuleBuilderOptions<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> rule) =>
        rule.NotEmpty().Length(PasswordMinLength, PasswordMaxLength);
}

internal sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName).ValidUserName();
        RuleFor(x => x.Password).ValidPassword();
    }
}

internal sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(ValidationRules.UserNameMaxLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(ValidationRules.PasswordMaxLength);
    }
}

internal sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Product.NameMaxLength);
        RuleFor(x => x.Description).MaximumLength(Product.DescriptionMaxLength);
        RuleFor(x => x.Price).GreaterThan(0).PrecisionScale(18, 2, ignoreTrailingZeros: true);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}

internal sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(Category.NameMaxLength);
    }
}

internal sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.UserName).ValidUserName();
        RuleFor(x => x.Password).ValidPassword();
        RuleFor(x => x.Role).Must(Roles.IsValid).WithMessage($"Role must be '{Roles.Admin}' or '{Roles.Customer}'.");
    }
}

internal sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.UserName).ValidUserName();
        RuleFor(x => x.Password).ValidPassword().When(x => !string.IsNullOrEmpty(x.Password));
        RuleFor(x => x.Role).Must(Roles.IsValid).WithMessage($"Role must be '{Roles.Admin}' or '{Roles.Customer}'.");
    }
}

internal sealed class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, Order.MaxQuantity);
    }
}

internal sealed class CreateMyOrderRequestValidator : AbstractValidator<CreateMyOrderRequest>
{
    public CreateMyOrderRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, Order.MaxQuantity);
    }
}

internal sealed class UpdateOrderRequestValidator : AbstractValidator<UpdateOrderRequest>
{
    public UpdateOrderRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).InclusiveBetween(1, Order.MaxQuantity);
    }
}
