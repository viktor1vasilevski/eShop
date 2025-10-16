using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CommentCustomerService(IEfUnitOfWork _uow, IEfRepository<Comment> _commentRepository, 
    IEfRepository<Order> _orderRepository) : ICommentCustomerService
{

    public async Task<ApiResponse<List<CommentCustomerDto>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Comment>(request.SortBy, request.SortDirection);

        var (comments, totalCount) = await _commentRepository.QueryAsync(
            queryBuilder: q => q.Where(c => c.ProductId == request.ProductId),
            selector: c => new CommentCustomerDto
            {
                Rating = c.Rating,
                Created = c.Created,
                CommentText = c.CommentText,
                CreatedBy = c.CreatedBy
            },
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return new ApiResponse<List<CommentCustomerDto>>
        {
            Data = comments.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CommentCustomerDto>> CreateCommentAsync(CreateCommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        bool hasBought = await _orderRepository
            .QueryAsync(
                q => q.Where(o => o.UserId == request.UserId &&
                                  o.OrderItems.Any(oi => oi.ProductId == request.ProductId)),
                selector: o => o.Id,
                cancellationToken: cancellationToken
            )
            .ContinueWith(t => t.Result.Items.Any(), cancellationToken);

        if (!hasBought)
            return new ApiResponse<CommentCustomerDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerCommentConstants.CannotCommentWithoutPurchase,
            };

        var comment = Comment.Create(
            request.CommentText,
            request.Rating,
            request.ProductId,
            request.UserId
        );

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var resultDto = new CommentCustomerDto
        {
            CommentText = comment.CommentText,
            CreatedBy = comment.CreatedBy,
            Created = comment.Created,
            Rating = comment.Rating
        };

        return new ApiResponse<CommentCustomerDto>
        {
            Status = ResponseStatus.Success,
            Data = resultDto
        };
    }



}
