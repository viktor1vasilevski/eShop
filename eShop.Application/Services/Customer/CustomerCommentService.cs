using eShop.Application.Constants.Customer;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Customer;

public class CustomerCommentService(IEfUnitOfWork _uow, IEfRepository<Comment> _commentRepository,
    IEfRepository<Order> _orderRepository) : ICustomerCommentService
{
    public async Task<Result<List<CommentCustomerDto>>> GetCommentsAsync(CommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Comment>(request.SortBy, request.SortDirection);

        var (comments, totalCount) = await _commentRepository.QueryAsync(
            queryBuilder: q => q.Where(c => c.ProductId == request.ProductId),
            includeBuilder: x => x.Include(x => x.User),
            selector: c => new CommentCustomerDto
            {
                Rating = c.Rating,
                Created = c.Created,
                CommentText = c.Text.Value,
                CreatedBy = c.User.Username.Value
            },
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return Result<List<CommentCustomerDto>>.Success(comments, totalCount);
    }

    public async Task<Result<CommentCustomerDto>> CreateCommentAsync(Guid userId, CreateCommentCustomerRequest request, CancellationToken cancellationToken = default)
    {
        bool hasBought = await _orderRepository
            .QueryAsync(
                q => q.Where(o => o.UserId == userId &&
                                  o.OrderItems.Any(oi => oi.ProductId == request.ProductId)),
                selector: o => o.Id,
                cancellationToken: cancellationToken
            )
            .ContinueWith(t => t.Result.Items.Any(), cancellationToken);

        if (!hasBought)
            return Result<CommentCustomerDto>.NotFound(CustomerCommentConstants.CannotCommentWithoutPurchase);

        var comment = Comment.Create(request.CommentText, request.Rating, request.ProductId, userId);

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<CommentCustomerDto>.Success(new CommentCustomerDto
        {
            CommentText = comment.Text.Value,
            CreatedBy = comment.CreatedBy,
            Created = comment.Created,
            Rating = comment.Rating
        });
    }
}
