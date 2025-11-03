using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICommentCustomerService
{
    Task<ApiResponse<List<CommentCustomerResponse>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommentCustomerResponse>> CreateCommentAsync(CreateCommentCustomerRequest request, CancellationToken cancellationToken = default);
}
