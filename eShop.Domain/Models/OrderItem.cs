using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;

namespace eShop.Domain.Models;

public class OrderItem : AuditableBaseEntity
{
    public Guid OrderId { get; private set; }
    public virtual Order? Order { get; private set; }

    public Guid ProductId { get; private set; }
    public virtual Product? Product { get; private set; }

    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }


    private OrderItem() { }

    public static OrderItem Create(Guid productId, int quantity, decimal price)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(productId, nameof(productId));

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = price
        };
    }
}
