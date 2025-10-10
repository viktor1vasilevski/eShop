using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CommentCustomerService(IUnitOfWork _uow) : ICommentCustomerService
{
    private readonly IEfRepository<Comment> _commentRepository = _uow.GetEfRepository<Comment>();
    private readonly IEfRepository<Order> _orderRepository = _uow.GetEfRepository<Order>();

    public async Task<ApiResponse<CommentCustomerDto>> CreateCommentAsync(CreateCommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<List<CommentCustomerDto>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
