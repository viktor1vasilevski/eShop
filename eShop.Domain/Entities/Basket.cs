using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Basket : AuditableBaseEntity
{
    public Guid UserId { get; set; }
    public virtual User? User { get; private set; }

    private readonly List<BasketItem> _items = [];
    public IReadOnlyCollection<BasketItem>? Items => _items.AsReadOnly();


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

    public void AddItem(Product product, int quantity)
    {
        if (product == null)
            throw new DomainException("Product cannot be null.");

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            // Update quantity but do not exceed product.UnitQuantity
            existingItem.Quantity = Math.Min(existingItem.Quantity + quantity, product.UnitQuantity);
        }
        else
        {
            var newItem = new BasketItem
            {
                ProductId = product.Id,
                Quantity = Math.Min(quantity, product.UnitQuantity),
                Basket = this // EF tracking
            };
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
