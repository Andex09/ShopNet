using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Common;
using ShopNet.Application.Features.Orders;
using ShopNet.Contracts.Orders;
using ShopNet.Domain.Orders;
using ShopNet.Domain.Products;
using ShopNet.UnitTests.Domain;
using ShopNet.UnitTests.Infrastructure;

namespace ShopNet.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly int[] MouseAndHeadphones = [2, 3];

    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUserAccounts _users = Substitute.For<IUserAccounts>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _handler = new CreateOrderHandler(
            _orders, _products, _users, _unitOfWork,
            Options.Create(new OrderOptions { TaxRate = 0.19m }),
            new FixedTimeProvider(Now),
            NullLogger<CreateOrderHandler>.Instance);

        _users.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        _products.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(call => Catalog.Where(p => call.Arg<IReadOnlyCollection<int>>().Contains(p.Id)).ToList());
    }

    private static List<Product> Catalog { get; } =
    [
        TestCatalog.Product("Mouse", 350_000m, id: 2),
        TestCatalog.Product("Audífonos", 200_000m, id: 3),
    ];

    [Fact]
    public async Task Uses_the_stored_product_prices_and_configured_tax()
    {
        var response = await _handler.HandleAsync(Request(userId: 1, (2, 2), (3, 1)), TestContext.Current.CancellationToken);

        response.Status.ShouldBe(OrderStatusNames.Pending);
        response.CreatedAt.ShouldBe(Now);
        response.Lines.Select(l => (l.ProductId, l.UnitPrice)).ShouldBe([(2, 350_000m), (3, 200_000m)]);
        response.TaxRate.ShouldBe(0.19m);
        response.Total.ShouldBe(1_071_000m);
        _orders.Received(1).Add(Arg.Is<Order>(o => o.Total == 1_071_000m));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Loads_all_products_in_one_query()
    {
        await _handler.HandleAsync(Request(userId: 1, (2, 1), (3, 1), (2, 1)), TestContext.Current.CancellationToken);

        await _products.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Order().SequenceEqual(MouseAndHeadphones)), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Throws_not_found_when_a_product_does_not_exist()
    {
        var error = await Should.ThrowAsync<NotFoundException>(() =>
            _handler.HandleAsync(Request(userId: 1, (2, 1), (99, 1)), TestContext.Current.CancellationToken));

        error.Message.ShouldContain("99");
        _orders.DidNotReceive().Add(Arg.Any<Order>());
    }

    [Fact]
    public async Task Throws_not_found_when_the_user_does_not_exist() =>
        await Should.ThrowAsync<NotFoundException>(() =>
            _handler.HandleAsync(Request(userId: 42, (2, 1)), TestContext.Current.CancellationToken));

    private static CreateOrderRequest Request(int userId, params (int ProductId, int Quantity)[] lines) => new()
    {
        UserId = userId,
        Lines = [.. lines.Select(l => new OrderLineRequest { ProductId = l.ProductId, Quantity = l.Quantity })],
    };
}
