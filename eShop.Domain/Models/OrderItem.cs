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

        var productQuantity = UnitQuantity.Create(quantity);
        var productPrice = UnitPrice.Create(price);

        var orderItem = new OrderItem();

        orderItem.Id = Guid.NewGuid();
        orderItem.ProductId = productId;
        orderItem.UnitQuantity = productQuantity;
        orderItem.UnitPrice = productPrice;

        return orderItem;
    }
}
