using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using ShopNet.Application.Abstractions;
using ShopNet.Application.Features.Products;
using ShopNet.UnitTests.Domain;

namespace ShopNet.UnitTests.Application;

public class DeleteProductHandlerTests
{
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IOrderRepository _orders = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteProductHandler _handler;

    public DeleteProductHandlerTests() =>
        _handler = new DeleteProductHandler(_products, _orders, _unitOfWork, NullLogger<DeleteProductHandler>.Instance);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Deactivates_a_product_that_has_orders()
    {
        var product = TestCatalog.Product("Mouse", 350_000m, id: 2);
        _products.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(product);
        _orders.AnyForProductAsync(2, Arg.Any<CancellationToken>()).Returns(true);

        await _handler.HandleAsync(2, Ct);

        product.IsActive.ShouldBeFalse();
        _products.DidNotReceive().Remove(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Removes_a_product_that_was_never_ordered()
    {
        var product = TestCatalog.Product("Mouse", 350_000m, id: 2);
        _products.GetByIdAsync(2, Arg.Any<CancellationToken>()).Returns(product);

        await _handler.HandleAsync(2, Ct);

        _products.Received(1).Remove(product);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
