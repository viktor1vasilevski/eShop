using eShop.Application.DTOs.Comment;
using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Comment;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;

namespace eShop.Application.Services;

public class CommentService(IUnitOfWork _uow) : ICommentService
{
    private readonly IRepositoryBase<Comment> _commentRepository = _uow.GetRepository<Comment>();
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>();

    public ApiResponse<CommentDTO> CreateComment(CreateCommentRequest request)
    {
        bool hasBought = _orderRepository.Exists(o =>
            o.UserId == request.UserId &&
            o.Items.Any(i => i.ProductId == request.ProductId));

        if (!hasBought)
            return new ApiResponse<CommentDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = "User cannot comment without buying the product."
            };

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            UserId = request.UserId,
            CommentText = request.CommentText
        };

        _commentRepository.Insert(comment);
        _uow.SaveChanges();


        var resultDto = new CommentDTO
        {
            CommentText = comment.CommentText,
        };

        return new ApiResponse<CommentDTO>
        {
            NotificationType = NotificationType.Success,
            Data = resultDto
        };
    }
}
