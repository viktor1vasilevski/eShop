using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Services.Customer;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Customer;

public class CustomerCommentServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<Comment>> _commentRepoMock = new();
    private readonly Mock<IEfRepository<Order>> _orderRepoMock = new();
    private readonly CustomerCommentService _sut;

    public CustomerCommentServiceTests()
    {
        _sut = new CustomerCommentService(_uowMock.Object, _commentRepoMock.Object, _orderRepoMock.Object);
    }

    [Fact]
    public async Task CreateCommentAsync_UserHasNotPurchasedProduct_ReturnsNotFound()
    {
        _orderRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Expression<Func<Order, Guid>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Guid>(), 0));

        var request = new CreateCommentCustomerRequest
        {
            ProductId = Guid.NewGuid(),
            CommentText = "Great product!",
            Rating = Rating.Excellent
        };

        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), request);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        Assert.Equal(CustomerCommentConstants.CannotCommentWithoutPurchase, result.Message);
    }

    [Fact]
    public async Task CreateCommentAsync_UserHasPurchasedProduct_ReturnsSuccess()
    {
        _orderRepoMock
            .Setup(r => r.QueryAsync(
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Expression<Func<Order, Guid>>>(),
                It.IsAny<Func<IQueryable<Order>, IQueryable<Order>>>(),
                It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Guid> { Guid.NewGuid() }, 1));

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var request = new CreateCommentCustomerRequest
        {
            ProductId = Guid.NewGuid(),
            CommentText = "Great product!",
            Rating = Rating.Excellent
        };

        var result = await _sut.CreateCommentAsync(Guid.NewGuid(), request);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal("Great product!", result.Data!.CommentText);
    }
}
