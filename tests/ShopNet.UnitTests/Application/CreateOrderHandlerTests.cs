using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.Contracts.Orders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.Domain.Users;

namespace ShopNet.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(
            _orders, _products, _users, _unitOfWork,
            Options.Create(new OrderOptions { TaxRate = 0.19m }),
            NullLogger<CreateOrderHandler>.Instance);

        _users.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(User.Create("ana", Roles.Customer, [1], [1]));
    }

    [Fact]
    public async Task Uses_the_stored_product_price_and_configured_tax()
    {
        _products.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(Product.Create("Mouse", 350_000m));

        var response = await _handler.HandleAsync(
            new CreateOrderRequest { UserId = 1, ProductId = 2, Quantity = 2 }, TestContext.Current.CancellationToken);

        response.UnitPrice.ShouldBe(350_000m);
        response.TaxRate.ShouldBe(0.19m);
        response.Total.ShouldBe(833_000m);
        _orders.Received(1).Add(Arg.Is<Order>(o => o.Total == 833_000m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_when_the_product_does_not_exist()
    {
        _products.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(
            new CreateOrderRequest { UserId = 1, ProductId = 99, Quantity = 1 }, TestContext.Current.CancellationToken));

        _orders.DidNotReceive().Add(Arg.Any<Order>());
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist()
    {
        await Should.ThrowAsync<NotFoundException>(() => _handler.HandleAsync(
            new CreateOrderRequest { UserId = 42, ProductId = 2, Quantity = 1 }, TestContext.Current.CancellationToken));
    }
}
