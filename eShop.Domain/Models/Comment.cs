using eShop.Domain.Enums;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Models;

public class Comment : AuditableBaseEntity
{
    public Guid ProductId { get; private set; }
    public virtual Product? Product { get; private set; }

    public Guid UserId { get; private set; }
    public virtual User? User { get; private set; }

    public CommentText? CommentText { get; private set; }
    public Rating Rating { get; private set; }


    private Comment() { }
    public static Comment Create(string commentText, Rating rating, Guid productId, Guid userId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(productId, nameof(productId));
        DomainValidatorHelper.ThrowIfEmptyGuid(userId, nameof(userId));

        return new Comment
        {
            Id = Guid.NewGuid(),
            CommentText = CommentText.Create(commentText),
            Rating = rating,
            ProductId = productId,
            UserId = userId,
        };
    }
}
