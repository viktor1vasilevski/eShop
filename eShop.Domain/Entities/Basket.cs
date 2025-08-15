using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User? User { get; private set; }

    private readonly List<BasketItem> _items = [];
    public IReadOnlyCollection<BasketItem>? Items => _items.AsReadOnly();


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

    public void ClearItems() => _items.Clear();

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
            _items.Remove(item);
    }

    public void AddOrUpdateItem(Product product, int quantityToAdd)
    {
        if (product == null)
            throw new DomainException("Product cannot be null.");

        if (quantityToAdd <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            int totalQuantity = existingItem.Quantity + quantityToAdd;
            existingItem.UpdateQuantity(totalQuantity, product.UnitQuantity);
        }
        else
        {
            var newItem = BasketItem.CreateNew(this.Id, product.Id, Math.Min(quantityToAdd, product.UnitQuantity));
            _items.Add(newItem);
        }
    }




    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem == null)
            throw new DomainException("Item not found in the basket.");

        existingItem.UpdateQuantity(quantity, existingItem.Quantity);
    }
}
