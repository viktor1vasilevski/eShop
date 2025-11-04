using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObject;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Models;

public class Product : AuditableBaseEntity
{
    public ProductName Name { get; private set; } = null!;
    public ProductDescription Description { get; private set; } = null!;
    public UnitPrice UnitPrice { get; private set; } = null!;
    public UnitQuantity UnitQuantity { get; private set; } = null!;
    public Image Image { get; private set; } = null!;
    public bool IsDeleted { get; private set; }

    public Guid CategoryId { get; private set; }
    public virtual Category Category { get; private set; } = null!;


    private readonly List<BasketItem> _basketItems = [];
    public virtual IReadOnlyCollection<BasketItem> BasketItems => _basketItems.AsReadOnly();


    private readonly List<OrderItem> _orderItems = [];
    public virtual IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();


    private readonly List<Comment> _comments = [];
    public virtual IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();


    private Product() { }

    public static Product Create(string name, string description, decimal unitPrice, int unitQuantity, Guid categoryId, Image image)
    {
        var instance = new Product();
        instance.Id = Guid.NewGuid();
        instance.Update(name, description, unitPrice, unitQuantity, categoryId, image);
        return instance;
    }

    public void Update(string name, string description, decimal unitPrice, int unitQuantity, Guid categoryId, Image image)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));

        Name = ProductName.Create(name);
        Description = ProductDescription.Create(description);
        UnitPrice = UnitPrice.Create(unitPrice);
        UnitQuantity = UnitQuantity.Create(unitQuantity);

        if (image != null)
        {
            Image = image;
        }

        CategoryId = categoryId;
        IsDeleted = false;
    }

    public void SubtrackQuantity(int requestedQuantity)
    {
        UnitQuantity = UnitQuantity.Subtract(requestedQuantity);
    }

    public void SoftDelete() => IsDeleted = true;
}
