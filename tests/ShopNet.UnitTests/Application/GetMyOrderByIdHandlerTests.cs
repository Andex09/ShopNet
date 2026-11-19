using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.UnitTests.Domain;

namespace ShopNet.UnitTests.Application;

public class GetMyOrderByIdHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly GetMyOrderByIdHandler _handler;

    public GetMyOrderByIdHandlerTests()
    {
        _handler = new GetMyOrderByIdHandler(_orders, _currentUser);
        _currentUser.UserId.Returns(7);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Returns_the_users_own_order()
    {
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(TestOrders.Pending(userId: 7));

        var response = await _handler.HandleAsync(1, Ct);

        response.UserId.ShouldBe(7);
        response.Lines.ShouldHaveSingleItem().ProductName.ShouldBe("Mouse");
    }

    [Fact]
    public async Task Reports_someone_elses_order_as_not_found()
    {
        _orders.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(TestOrders.Pending(userId: 8));

        await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(1, Ct));
    }
}
