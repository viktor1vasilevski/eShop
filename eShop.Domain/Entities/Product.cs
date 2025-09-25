using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Entities;

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

        if (image != null) // 👈 only update if provided
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
