using ShopNet.Application.Features;
using ShopNet.Contracts.Orders;
using ShopNet.Domain.Orders;

namespace ShopNet.UnitTests.Application;

public class OrderRequestValidatorTests
{
    private readonly CreateMyOrderRequestValidator _validator = new();

    [Theory]
    [InlineData(0, 1)]
    [InlineData(2, 0)]
    [InlineData(2, -1)]
    [InlineData(2, Order.MaxQuantityPerLine + 1)]
    public void Rejects_invalid_lines(int productId, int quantity) =>
        _validator.Validate(Request([(productId, quantity)])).IsValid.ShouldBeFalse();

    [Fact]
    public void Rejects_an_order_without_lines() =>
        _validator.Validate(Request([])).IsValid.ShouldBeFalse();

    [Fact]
    public void Rejects_more_lines_than_allowed() =>
        _validator.Validate(Request([.. Enumerable.Range(1, Order.MaxLines + 1).Select(id => (id, 1))])).IsValid.ShouldBeFalse();

    [Fact]
    public void Accepts_valid_lines() =>
        _validator.Validate(Request([(2, 1), (3, Order.MaxQuantityPerLine)])).IsValid.ShouldBeTrue();

    private static CreateMyOrderRequest Request(List<(int ProductId, int Quantity)> lines) => new()
    {
        Lines = [.. lines.Select(l => new OrderLineRequest { ProductId = l.ProductId, Quantity = l.Quantity })],
    };
}
