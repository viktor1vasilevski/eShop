using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObject;

namespace eShop.Domain.Models;

public class Product : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public int UnitQuantity { get; private set; }
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
        Validate(name, description, unitPrice, unitQuantity, categoryId);

        Name = name;
        Description = description;
        UnitPrice = unitPrice;
        UnitQuantity = unitQuantity;

        if (image != null)
        {
            Image = image;
        }

        CategoryId = categoryId;
        IsDeleted = false;
    }

    public void SubtrackQuantity(int requestedQuantity)
    {
        UnitQuantity -= requestedQuantity;
    }

    public void SoftDelete() => IsDeleted = true;

    private void Validate(string name, string description, decimal unitPrice, int unitQuantity, Guid categoryId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(description, nameof(description));

        if (name.Length > 50)
            throw new DomainValidationException("Product name cannot exceed 50 characters.");

        if (description.Length > 2500)
            throw new DomainValidationException("Description cannot exceed 2500 characters.");

        if (unitPrice <= 0)
            throw new DomainValidationException("Unit price must be greater than zero.");

        if (unitQuantity <= 0)
            throw new DomainValidationException("Unit quantity must be greater than zero.");
    }
}
