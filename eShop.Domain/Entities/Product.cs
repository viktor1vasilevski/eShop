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

    public Guid SubcategoryId { get; private set; }
    public virtual Subcategory? Subcategory { get; set; }
    public ICollection<BasketItem>? BasketItems { get; set; }
    public virtual ICollection<OrderItem>? OrderItems { get; set; }


    protected Product() { }

    public static Product CreateNew(ProductData product)
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

    private void ApplyProductData(ProductData product)
    {
        var imageBytes = ConvertBase64ToBytes(product.Base64Image);
        var imageType = ExtractImageType(product.Base64Image);

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

        if (product.Description.Length > 500)
            throw new DomainValidationException("Description cannot exceed 500 characters.");

        if (product.UnitPrice <= 0)
            throw new DomainValidationException("Unit price must be greater than zero.");

        if (product.UnitQuantity <= 0)
            throw new DomainValidationException("Unit quantity must be greater than zero.");

        ValidateImage(imageBytes, imageType);
    }

    private void ValidateImage(byte[] imageBytes, string imageType)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new DomainValidationException("Image must be provided.");

        const int maxImageSizeInBytes = 5 * 1024 * 1024; // 5MB
        if (imageBytes.Length > maxImageSizeInBytes)
            throw new DomainValidationException("Image size cannot exceed 5MB.");

        if (string.IsNullOrWhiteSpace(imageType))
            throw new DomainValidationException("Image type must be provided.");

        var allowedTypes = new[] { "jpeg", "png", "webp" };
        if (!allowedTypes.Contains(imageType.ToLower()))
            throw new DomainValidationException($"Unsupported image type: {imageType}");
    }

    private byte[] ConvertBase64ToBytes(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return Array.Empty<byte>();

        string base64Data = base64String.Contains("base64,")
            ? base64String.Substring(base64String.IndexOf("base64,") + 7)
            : base64String;

        return Convert.FromBase64String(base64Data);
    }

    private string ExtractImageType(string base64String)
    {
        if (string.IsNullOrEmpty(base64String)) return string.Empty;

        var parts = base64String.Split(';');
        if (parts.Length == 0 || !parts[0].Contains("/")) return string.Empty;

        var typeParts = parts[0].Split('/');
        return typeParts.Length > 1 ? typeParts[1] : string.Empty;
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
