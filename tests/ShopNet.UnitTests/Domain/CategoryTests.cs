using ShopNet.Domain.Categories;
using ShopNet.Domain.Common;

namespace ShopNet.UnitTests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_trims_the_name() =>
        Category.Create("  Audio  ").Name.ShouldBe("Audio");

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_rejects_an_empty_name(string name) =>
        Should.Throw<DomainException>(() => Category.Create(name));

    [Fact]
    public void Create_rejects_a_name_that_is_too_long() =>
        Should.Throw<DomainException>(() => Category.Create(new string('a', Category.NameMaxLength + 1)));
}
