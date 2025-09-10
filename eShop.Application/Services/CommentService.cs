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


    public ApiResponse<List<CommentDTO>> GetComments(CommentRequest request)
    {
        var query = _commentRepository.GetAsQueryable(x => x.ProductId == request.ProductId);

        var totalCount = query.Count();

        var sortedQuery = query;
        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderBy(x => x.Created),
                    _ => sortedQuery.OrderBy(x => x.Created)
                };
            }
            else if (request.SortDirection.ToLower() == "desc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderByDescending(x => x.Created),
                    _ => sortedQuery.OrderByDescending(x => x.Created)
                };
            }
        }

        if (request.Skip.HasValue)
            sortedQuery = sortedQuery.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            sortedQuery = sortedQuery.Take(request.Take.Value);

        var commentsDTO = sortedQuery.Select(x => new CommentDTO
        {
            Rating = x.Rating,
            Created = x.Created,
            CommentText = x.CommentText,
            CreatedBy = x.CreatedBy
        }).ToList();

        return new ApiResponse<List<CommentDTO>>()
        {
            Data = commentsDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success,
        };
    }
    public ApiResponse<CommentDTO> CreateComment(CreateCommentRequest request)
    {
        bool hasBought = _orderRepository.Exists(o =>
            o.UserId == request.UserId &&
            o.OrderItems.Any(i => i.ProductId == request.ProductId));

        if (!hasBought)
            return new ApiResponse<CommentDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = "User cannot comment without buying the product."
            };

        var comment = Comment.Create(request.CommentText, request.Rating, request.ProductId, request.UserId);

        _commentRepository.Insert(comment);
        _uow.SaveChanges();

        var resultDto = new CommentDTO
        {
            CommentText = comment.CommentText,
            CreatedBy = comment.CreatedBy,
            Created = comment.Created,
            Rating = comment.Rating,
        };

        return new ApiResponse<CommentDTO>
        {
            Status = ResponseStatus.Success,
            Data = resultDto
        };
    }


}
