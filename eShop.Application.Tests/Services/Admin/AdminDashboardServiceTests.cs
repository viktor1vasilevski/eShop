using eShop.Application.Enums;
using eShop.Application.Services.Admin;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Admin;

public class AdminDashboardServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IEfRepository<User>> _userRepoMock = new();
    private readonly AdminDashboardService _sut;

    public AdminDashboardServiceTests()
    {
        _sut = new AdminDashboardService(_uowMock.Object, _orderRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task GetOrdersTodayAsync_NoOrders_ReturnsZeroCount()
    {
        _orderRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Expression<Func<Order, Guid>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Guid>(), 0));

        var result = await _sut.GetOrdersTodayAsync();

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(0, result.Data!.Count);
    }

    [Fact]
    public async Task GetOrdersTodayAsync_HasOrders_ReturnsCorrectCount()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        _orderRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Expression<Func<Order, Guid>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ids, ids.Count));

        var result = await _sut.GetOrdersTodayAsync();

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(2, result.Data!.Count);
    }

    [Fact]
    public async Task GetRevenueTodayAsync_HasOrders_ReturnsSummedRevenue()
    {
        var amounts = new List<decimal> { 100m, 250m, 50m };

        _orderRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Expression<Func<Order, decimal>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((amounts, amounts.Count));

        var result = await _sut.GetRevenueTodayAsync();

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(400m, result.Data!.Amount);
    }

    [Fact]
    public async Task GetTotalCustomersAsync_ReturnsCorrectCount()
    {
        var ids = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _userRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<Expression<Func<User, Guid>>>(),
                It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(),
                It.IsAny<Func<IQueryable<User>, IOrderedQueryable<User>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ids, ids.Count));

        var result = await _sut.GetTotalCustomersAsync();

        Assert.Equal(ResponseStatus.Success, result.Status);
        Assert.Equal(3, result.Data!.Count);
    }
}
