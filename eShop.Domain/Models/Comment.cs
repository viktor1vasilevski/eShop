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

    public CommentText Text { get; private set; } = null!;
    public Rating Rating { get; private set; }


    private Comment() { }
    public static Comment Create(string commentText, Rating rating, Guid productId, Guid userId)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(commentText, nameof(commentText));
        DomainValidatorHelper.ThrowIfEmptyGuid(productId, nameof(productId));
        DomainValidatorHelper.ThrowIfEmptyGuid(userId, nameof(userId));

        var text = CommentText.Create(commentText);

        return new Comment
        {
            Id = Guid.NewGuid(),
            Text = text,
            Rating = rating,
            ProductId = productId,
            UserId = userId,
        };
    }
}
