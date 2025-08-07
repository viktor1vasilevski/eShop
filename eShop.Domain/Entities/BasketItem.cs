using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class BasketItem : AuditableBaseEntity
{
    public Guid BasketId { get; set; }
    public virtual Basket? Basket { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public int Quantity { get; set; }
}
