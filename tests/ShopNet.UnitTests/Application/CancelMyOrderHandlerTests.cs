using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.Domain.Orders;
using ShopNet.UnitTests.Domain;

namespace ShopNet.UnitTests.Application;

public class CancelMyOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CancelMyOrderHandler _handler;

    public CancelMyOrderHandlerTests()
    {
        _handler = new CancelMyOrderHandler(_orders, _currentUser, _unitOfWork, NullLogger<CancelMyOrderHandler>.Instance);
        _currentUser.UserId.Returns(7);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cancels_the_users_own_pending_order()
    {
        var order = TestOrders.Pending(userId: 7);
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await _handler.HandleAsync(1, Ct);

        order.Status.ShouldBe(OrderStatus.Cancelled);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reports_someone_elses_order_as_not_found_and_leaves_it_untouched()
    {
        var order = TestOrders.Pending(userId: 8);
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(1, Ct));

        order.Status.ShouldBe(OrderStatus.Pending);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cannot_cancel_a_confirmed_order()
    {
        var order = TestOrders.Pending(userId: 7);
        order.Confirm();
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(order);

        await Should.ThrowAsync<OrderStateException>(() => _handler.HandleAsync(1, Ct));
    }
}
