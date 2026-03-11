using eShop.Application.Constants.Customer;
using eShop.Application.DTOs.Customer.OrderItem;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Services.Customer;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Customer;

public class CustomerOrderServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IEfRepository<User>> _userRepoMock = new();
    private readonly Mock<IEfRepository<Product>> _productRepoMock = new();
    private readonly Mock<ILogger<CustomerOrderService>> _loggerMock = new();
    private readonly Mock<IEmailQueue> _emailQueueMock = new();
    private readonly CustomerOrderService _sut;

    public CustomerOrderServiceTests()
    {
        _sut = new CustomerOrderService(
            _uowMock.Object,
            _orderRepoMock.Object,
            _userRepoMock.Object,
            _productRepoMock.Object,
            _loggerMock.Object,
            _emailQueueMock.Object);
    }

    [Fact]
    public async Task PlaceOrderAsync_UserNotFound_ReturnsNotFound()
    {
        _userRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, User>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.PlaceOrderAsync(Guid.NewGuid(), new PlaceOrderCustomerRequest
        {
            TotalAmount = 100m,
            Items = [new PlaceOrderItemCustomerDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }]
        });

        Assert.Equal(ResponseStatus.NotFound, result.Status);
        Assert.Equal(CustomerAuthConstants.UserNotFound, result.Message);
    }

    [Fact]
    public async Task PlaceOrderAsync_ProductNotFound_ReturnsNotFound()
    {
        var user = CreateTestUser();

        _userRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, User>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Return empty product list — product not found
        _productRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Expression<Func<Product, Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product>(), 0));

        var result = await _sut.PlaceOrderAsync(user.Id, new PlaceOrderCustomerRequest
        {
            TotalAmount = 100m,
            Items = [new PlaceOrderItemCustomerDto { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 100m }]
        });

        Assert.Equal(ResponseStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task PlaceOrderAsync_ValidRequest_ReturnsSuccess()
    {
        var user = CreateTestUser();
        var product = CreateTestProduct();

        _userRepoMock
            .Setup(r => r.GetSingleAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<Expression<Func<User, User>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _productRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Expression<Func<Product, Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IQueryable<Product>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Product> { product }, 1));

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _emailQueueMock.Setup(e => e.EnqueueAsync(It.IsAny<EmailMessage>())).Returns(ValueTask.CompletedTask);

        var result = await _sut.PlaceOrderAsync(user.Id, new PlaceOrderCustomerRequest
        {
            TotalAmount = 10m,
            Items = [new PlaceOrderItemCustomerDto { ProductId = product.Id, Quantity = 1, UnitPrice = 10m }]
        });

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(CustomerOrderConstants.OrderPlaced, result.Message);
        Assert.Single(result.Data!.Items);
    }

    private static User CreateTestUser() =>
        User.CreateNew(new UserData("John", "Doe", "john", "john@test.com", "hash", "salt", Role.Customer));

    private static Product CreateTestProduct()
    {
        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        return Product.Create("Laptop", "A laptop", 10m, 5, Guid.NewGuid(), image);
    }
}
