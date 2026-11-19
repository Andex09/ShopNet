using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.Contracts.Orders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.UnitTests.Domain;

namespace ShopNet.UnitTests.Application;

public class UpdateOrderHandlerTests
{
    private static readonly Product Headphones = TestCatalog.Product("Audífonos", 200_000m, id: 3);

    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly UpdateOrderHandler _handler;

    public UpdateOrderHandlerTests()
    {
        _handler = new UpdateOrderHandler(_orders, _products, _unitOfWork);
        _products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<IReadOnlyCollection<int>>().Contains(Headphones.Id) ? [Headphones] : []);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Replaces_the_lines_and_recalculates_the_amounts()
    {
        var order = TestOrders.Pending();
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await _handler.HandleAsync(1, Request((3, 2)), Ct);

        order.Lines.ShouldHaveSingleItem().ProductName.ShouldBe("Audífonos");
        order.Subtotal.ShouldBe(400_000m);
        order.Total.ShouldBe(476_000m);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cannot_change_a_confirmed_order()
    {
        var order = TestOrders.Pending();
        order.Confirm();
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await Should.ThrowAsync<OrderStateException>(() => _handler.HandleAsync(1, Request((3, 2)), Ct));

        order.Lines.ShouldHaveSingleItem().ProductName.ShouldBe("Mouse");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_for_an_unknown_product()
    {
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(TestOrders.Pending());

        var error = await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(1, Request((99, 1)), Ct));

        error.Message.ShouldContain("99");
    }

    private static UpdateOrderRequest Request(params (int ProductId, int Quantity)[] lines) => new()
    {
        Lines = [.. lines.Select(l => new OrderLineRequest { ProductId = l.ProductId, Quantity = l.Quantity })],
    };
}
