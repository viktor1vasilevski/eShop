using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
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

    public static BasketItem Create(Guid basketId, Guid productId, int quantity)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(basketId, nameof(basketId));
        DomainValidatorHelper.ThrowIfEmptyGuid(productId, nameof(productId));

        var productQuantity = UnitQuantity.Create(quantity);

        var basketItem = new BasketItem();

        basketItem.BasketId = basketId;
        basketItem.ProductId = productId;
        basketItem.UnitQuantity = productQuantity;

        return basketItem;
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
