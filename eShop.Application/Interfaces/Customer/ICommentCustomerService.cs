using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses;
using eShop.Application.Responses.Customer.Comment;

namespace eShop.Application.Interfaces.Customer;

public interface ICommentCustomerService
{
    ApiResponse<List<CommentCustomerDto>> GetComments(CommentRequest request);
    ApiResponse<CommentCustomerDto> CreateComment(CreateCommentRequest request);
}
