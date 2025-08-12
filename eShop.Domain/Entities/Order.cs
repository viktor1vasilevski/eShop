using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Order : AuditableBaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual ICollection<OrderItem>? Items { get; set; }
}