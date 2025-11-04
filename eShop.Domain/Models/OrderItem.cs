using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Models;

public class OrderItem : AuditableBaseEntity
{
    public Guid OrderId { get; private set; }
    public virtual Order? Order { get; private set; }

    public Guid ProductId { get; private set; }
    public virtual Product? Product { get; private set; }

    public UnitQuantity UnitQuantity { get; private set; }
    public UnitPrice UnitPrice { get; private set; }


    private OrderItem() { }

    public static OrderItem Create(Guid productId, int quantity, decimal price)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(productId, nameof(productId));

        var unitQuantity = UnitQuantity.Create(quantity);
        var unitPrice = UnitPrice.Create(price);

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            UnitQuantity = unitQuantity,
            UnitPrice = unitPrice
        };
    }
}
