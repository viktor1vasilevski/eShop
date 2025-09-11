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



    //private readonly List<BasketItem> _basketItems = [];
    //public virtual ICollection<BasketItem>? BasketItems => _basketItems.AsReadOnly();


    //private readonly List<OrderItem> _orderItems = [];
    //public virtual ICollection<OrderItem>? OrderItems => _orderItems.AsReadOnly();


    //private readonly List<Comment> _comments = [];
    //public virtual ICollection<Comment>? Comments => _comments.AsReadOnly();


    private Product() { }

    public static Product Create(ProductData product)
    {
        var instance = new Product();
        instance.Id = Guid.NewGuid();
        instance.ApplyProductData(product);
        return instance;
    }

    public void Update(ProductData product)
    {
        ApplyProductData(product);
    }

    public void SubtrackQuantity(int requestedQuantity)
    {
        UnitQuantity -= requestedQuantity;
    }

    public void SoftDelete() => IsDeleted = true;

    private void ApplyProductData(ProductData product)
    {
        Validate(product);

        Name = product.Name;
        Description = product.Description;
        UnitPrice = product.UnitPrice;
        UnitQuantity = product.UnitQuantity;
        Image = product.Image;
    }

    private void Validate(ProductData product)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(product.SubcategoryId, nameof(product.SubcategoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(product.Name, nameof(product.Name));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(product.Description, nameof(product.Description));

        if (product.Name.Length > 50)
            throw new DomainValidationException("Brand name cannot exceed 50 characters.");

        if (product.Description.Length > 1500)
            throw new DomainValidationException("Description cannot exceed 1500 characters.");

        if (product.UnitPrice <= 0)
            throw new DomainValidationException("Unit price must be greater than zero.");

        if (product.UnitQuantity <= 0)
            throw new DomainValidationException("Unit quantity must be greater than zero.");
    }
}

public class ProductData
{
    public string Name { get; }
    public string Description { get; }
    public decimal UnitPrice { get; }
    public int UnitQuantity { get; }
    public Guid SubcategoryId { get; }
    public Image Image { get; }

    public ProductData(
        string name,
        string description,
        decimal unitPrice,
        int unitQuantity,
        Guid subcategoryId,
        Image image)
    {
        Name = name;
        Description = description;
        UnitPrice = unitPrice;
        UnitQuantity = unitQuantity;
        SubcategoryId = subcategoryId;
        Image = image;
    }
}
