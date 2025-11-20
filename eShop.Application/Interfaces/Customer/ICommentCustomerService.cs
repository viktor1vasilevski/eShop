using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICommentCustomerService
{
    Task<ApiResponse<List<CommentCustomerDto>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommentCustomerDto>> CreateCommentAsync(Guid userId, CreateCommentCustomerRequest request, CancellationToken cancellationToken = default);
}
