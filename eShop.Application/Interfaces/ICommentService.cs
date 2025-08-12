using eShop.Application.DTOs.Comment;
using eShop.Application.Requests.Comment;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ICommentService
{
    ApiResponse<CommentDTO> CreateComment(CreateCommentRequest request);
}
