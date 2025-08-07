using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; set; }

    public virtual User? User { get; set; }
    public virtual ICollection<BasketItem> Items { get; set; } = [];
}
