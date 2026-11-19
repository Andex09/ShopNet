using ShopNet.Application.Features;
using ShopNet.Contracts.Products;

namespace ShopNet.UnitTests.Application;

public class ProductRequestValidatorTests
{
    private readonly ProductRequestValidator _validator = new();

    [Theory]
    [InlineData("", 10)]
    [InlineData("Teclado", 0)]
    [InlineData("Teclado", 10.123)]
    public void Rejects_invalid_input(string name, double price)
    {
        var result = _validator.Validate(new ProductRequest { Name = name, Price = (decimal)price, CategoryId = 2 });

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Rejects_a_missing_category()
    {
        var result = _validator.Validate(new ProductRequest { Name = "Teclado", Price = 150_000m });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ProductRequest.CategoryId));
    }

    [Fact]
    public void Accepts_valid_input()
    {
        var result = _validator.Validate(new ProductRequest { Name = "Teclado", Price = 150_000m, CategoryId = 2 });

        result.IsValid.ShouldBeTrue();
    }
}
