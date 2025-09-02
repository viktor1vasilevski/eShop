using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Product : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; } 
    public int UnitQuantity { get; private set; }
    public byte[] Image { get; private set; } = [];
    public string ImageType { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public Guid SubcategoryId { get; private set; }
    public virtual Subcategory? Subcategory { get; set; }


    private readonly List<BasketItem> _basketItems = [];
    public virtual ICollection<BasketItem>? BasketItems => _basketItems.AsReadOnly();


    private readonly List<OrderItem> _orderItems = [];
    public virtual ICollection<OrderItem>? OrderItems => _orderItems.AsReadOnly();


    private readonly List<Comment> _comments = [];
    public virtual ICollection<Comment>? Comments => _comments.AsReadOnly();


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
        var imageBytes = ImageHelper.ConvertBase64ToBytes(product.Base64Image);
        var imageType = ImageHelper.ExtractImageType(product.Base64Image);

        Validate(product, imageBytes, imageType);

        Name = product.Name;
        Description = product.Description;
        UnitPrice = product.UnitPrice;
        UnitQuantity = product.UnitQuantity;
        SubcategoryId = product.SubcategoryId;
        Image = imageBytes;
        ImageType = imageType;
    }

    private void Validate(ProductData product, byte[] imageBytes, string imageType)
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

        ImageHelper.ValidateImage(imageBytes, imageType);
    }
}

public class ProductData
{
    public string Name { get; }
    public string Description { get; }
    public decimal UnitPrice { get; }
    public int UnitQuantity { get; }
    public Guid SubcategoryId { get; }
    public string Base64Image { get; }

    public ProductData(
        string name,
        string description,
        decimal unitPrice,
        int unitQuantity,
        Guid subcategoryId,
        string base64Image)
    {
        Name = name;
        Description = description;
        UnitPrice = unitPrice;
        UnitQuantity = unitQuantity;
        SubcategoryId = subcategoryId;
        Base64Image = base64Image;
    }
}
