using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Comment : AuditableBaseEntity
{
    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public string? CommentText { get; set; }
}
