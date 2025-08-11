using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Comment : AuditableBaseEntity
{
    public Guid OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public string? CommentText { get; set; }
}
