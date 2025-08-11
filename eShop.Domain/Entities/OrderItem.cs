using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class OrderItem : AuditableBaseEntity
{
    public Guid OrderId { get; set; }
    public virtual Order? Order { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

}
