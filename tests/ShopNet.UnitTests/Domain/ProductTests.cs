using ShopNet.Domain.Categories;
using ShopNet.Domain.Common;
using ShopNet.Domain.Products;

namespace ShopNet.UnitTests.Domain;

public class ProductTests
{
    private static readonly Category Audio = Category.Create("Audio");

    [Fact]
    public void Create_trims_the_texts_and_starts_active()
    {
        var product = Product.Create("  Teclado  ", "  Mecánico, inalámbrico  ", 100m, Audio);

        product.Name.ShouldBe("Teclado");
        product.Description.ShouldBe("Mecánico, inalámbrico");
        product.Category.ShouldBe(Audio);
        product.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Create_accepts_a_missing_description() =>
        Product.Create("Teclado", null, 100m, Audio).Description.ShouldBeEmpty();

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Create_rejects_non_positive_price(int price) =>
        Should.Throw<DomainException>(() => Product.Create("Teclado", null, price, Audio));

    [Fact]
    public void Create_rejects_empty_name() =>
        Should.Throw<DomainException>(() => Product.Create(" ", null, 100m, Audio));

    [Fact]
    public void Create_rejects_a_description_that_is_too_long() =>
        Should.Throw<DomainException>(() =>
            Product.Create("Teclado", new string('a', Product.DescriptionMaxLength + 1), 100m, Audio));

    [Fact]
    public void Update_moves_the_product_to_another_category()
    {
        var product = Product.Create("Teclado", null, 100m, Audio);
        var peripherals = Category.Create("Periféricos");

        product.Update("Teclado", null, 120m, peripherals);

        product.Category.ShouldBe(peripherals);
        product.Price.ShouldBe(120m);
    }

    [Fact]
    public void Can_be_deactivated_and_activated_again()
    {
        var product = Product.Create("Teclado", null, 100m, Audio);

        product.Deactivate();
        product.IsActive.ShouldBeFalse();

        product.Activate();
        product.IsActive.ShouldBeTrue();
    }
}
