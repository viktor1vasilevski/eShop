using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;

namespace eShop.Domain.Entities;

public class BasketItem : AuditableBaseEntity
{
    public Guid BasketId { get; set; }
    public virtual Basket? Basket { get; set; }

    public Guid ProductId { get; set; }
    public virtual Product? Product { get; set; }

    public int Quantity { get; set; }




    public static BasketItem CreateNew(Guid basketId, Guid productId, int quantity)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty.");
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        return new BasketItem
        {
            Id = Guid.NewGuid(),
            BasketId = basketId,
            ProductId = productId,
            Quantity = quantity
        };
    }

    public void UpdateQuantity(int newQuantity, int? maxQuantity = null)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (maxQuantity.HasValue)
            Quantity = Math.Min(newQuantity, maxQuantity.Value);
        else
            Quantity = newQuantity;
    }

}
