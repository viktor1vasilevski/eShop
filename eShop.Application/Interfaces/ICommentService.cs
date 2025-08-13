using eShop.Application.DTOs.Comment;
using eShop.Application.Requests.Comment;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface ICommentService
{
    ApiResponse<List<CommentDTO>> GetComments(CommentRequest request);
    ApiResponse<CommentDTO> CreateComment(CreateCommentRequest request);
}
