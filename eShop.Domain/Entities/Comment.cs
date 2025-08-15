using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Comment : AuditableBaseEntity
{
    public Guid ProductId { get; private set; }
    public virtual Product? Product { get; private set; }

    public Guid UserId { get; private set; }
    public virtual User? User { get; private set; }

    public string? CommentText { get; private set; }
    public int Rating { get; private set; }


    private Comment() { }
    public static Comment Create(string commentText, int rating, Guid productId, Guid userId)
    {
        return new Comment
        {
            Id = Guid.NewGuid(),
            CommentText = commentText,
            Rating = rating,
            ProductId = productId,
            UserId = userId,
        };
    }
}
