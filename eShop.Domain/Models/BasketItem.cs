using eShop.Domain.Exceptions;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Models;

public class BasketItem : AuditableBaseEntity
{
    public Guid BasketId { get; private set; }
    public virtual Basket? Basket { get; private set; }

    public Guid ProductId { get; private set; }
    public virtual Product? Product { get; private set; }

    public UnitQuantity UnitQuantity { get; private set; }


    private BasketItem() { }

    public static BasketItem CreateNew(Guid basketId, Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty.");

        return new BasketItem
        {
            BasketId = basketId,
            ProductId = productId,
            UnitQuantity = UnitQuantity.Create(quantity)
        };
    }

    public void UpdateQuantity(int newQuantity, int? maxQuantity = null)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var finalQuantity = maxQuantity.HasValue
            ? Math.Min(newQuantity, maxQuantity.Value)
            : newQuantity;

        UnitQuantity = UnitQuantity.Create(finalQuantity);
    }


}
