using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.Domain.Orders;
using ShopNet.UnitTests.Domain;

namespace ShopNet.UnitTests.Application;

public class ConfirmOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ConfirmOrderHandler _handler;

    public ConfirmOrderHandlerTests() =>
        _handler = new ConfirmOrderHandler(_orders, _unitOfWork, NullLogger<ConfirmOrderHandler>.Instance);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Confirms_a_pending_order_and_saves()
    {
        var order = TestOrders.Pending();
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await _handler.HandleAsync(1, Ct);

        order.Status.ShouldBe(OrderStatus.Confirmed);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Does_not_save_when_the_order_was_cancelled()
    {
        var order = TestOrders.Pending();
        order.Cancel();
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await Should.ThrowAsync<OrderStateException>(() => _handler.HandleAsync(1, Ct));

        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_for_a_missing_order() =>
        await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(99, Ct));
}
