using eShop.Application.DTOs.Comment;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces.Customer;

public interface ICommentCustomerService
{
    ApiResponse<List<CommentDTO>> GetComments(CommentRequest request);
}
