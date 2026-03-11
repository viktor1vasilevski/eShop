using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerCommentService
{
    Task<Result<List<CommentCustomerDto>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<CommentCustomerDto>> CreateCommentAsync(Guid userId, CreateCommentCustomerRequest request, CancellationToken cancellationToken = default);
}
