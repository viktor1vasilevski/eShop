using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;

namespace eShop.Domain.Models;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    private readonly List<BasketItem> _basketItems = [];
    public IReadOnlyCollection<BasketItem>? BasketItems => _basketItems.AsReadOnly();


    private Basket() { }
    public static Basket CreateNew(Guid userId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(userId, nameof(userId));

        return new Basket
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };
    }

    public void ClearItems() => _basketItems.Clear();

    public void RemoveItem(Guid productId)
    {
        var item = _basketItems.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
            _basketItems.Remove(item);
    }

    public void AddOrUpdateItem(Product product, int quantityToAdd)
    {
        if (product == null)
            throw new DomainException("Product cannot be null.");

        // if (quantityToAdd <= 0)
        //throw new DomainException("Quantity must be greater than zero.");

        var existingItem = _basketItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            int totalQuantity = existingItem.Quantity + quantityToAdd;
            existingItem.UpdateQuantity(totalQuantity, product.UnitQuantity);
        }
        else
        {
            var newItem = BasketItem.CreateNew(this.Id, product.Id, Math.Min(quantityToAdd, product.UnitQuantity));
            _basketItems.Add(newItem);
        }
    }




    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var existingItem = _basketItems.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem == null)
            throw new DomainException("Item not found in the basket.");

        existingItem.UpdateQuantity(quantity, existingItem.Quantity);
    }
}
