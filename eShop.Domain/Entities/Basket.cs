using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models;

namespace eShop.Domain.Entities;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; set; }

    public virtual User? User { get; private set; }
    public virtual ICollection<BasketItem> Items { get; set; } = [];


    public static Basket CreateNew(Guid userId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(userId, nameof(userId));

        return new Basket
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
    }

    public void MergeItems(IEnumerable<BasketMergeItem> items)
    {
        foreach (var item in items)
        {
            AddOrUpdateItem(item.Product, item.Quantity);
        }
    }



    public void AddOrUpdateItem(Product product, int quantity)
    {
        if (product == null)
            throw new DomainException("Product cannot be null.");
        //if (quantity <= 0)
        //    throw new DomainException("Quantity must be greater than zero.");

        var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
            existingItem.UpdateQuantity(existingItem.Quantity + quantity, product.UnitQuantity);
        else
            Items.Add(BasketItem.CreateNew(Id, product.Id, Math.Min(quantity, product.UnitQuantity)));
    }

    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId cannot be empty.");

        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
            throw new DomainException("Basket item not found.");

        item.UpdateQuantity(newQuantity, item.Product?.UnitQuantity ?? int.MaxValue);
    }


}
