using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace eShop.Domain.Entities;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public byte[] Image { get; private set; } = [];
    public string ImageType { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public Guid CategoryId { get; private set; }
    public virtual Category? Category { get; private set; }


    private readonly List<Product> _products = [];
    public virtual IReadOnlyCollection<Product>? Products => _products.AsReadOnly();


    private Subcategory() { }

    public static Subcategory Create(Guid categoryId, string name, string base64Image)
    {
        var instance = new Subcategory();
        instance.Id = Guid.NewGuid();
        instance.ApplySubcategoryData(categoryId, name, base64Image);
        return instance;
    }

    public void Update(Guid categoryId, string name, string? base64Image)
    {
        ApplySubcategoryData(categoryId, name, base64Image);
    }

    private void ApplySubcategoryData(Guid categoryId, string name, string? base64Image)
    {
        var (imageBytes, imageType) = ProcessImage(base64Image);
        Validate(categoryId, name, imageBytes, imageType);

        CategoryId = categoryId;
        Name = name;
        Image = imageBytes;
        ImageType = imageType;
    }

    private static void Validate(Guid categoryId, string name, byte[] imageBytes, string imageType)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        ImageHelper.ValidateImage(imageBytes, imageType);
    }

    private static (byte[] ImageBytes, string ImageType) ProcessImage(string? base64Image)
    {
        var imageBytes = ImageHelper.ConvertBase64ToBytes(base64Image);
        var imageType = ImageHelper.ExtractImageType(base64Image);
        return (imageBytes, imageType);
    }

    public void SoftDelete() => IsDeleted = true;

    public bool HasRelatedProducts()
    {
        return _products?.Any(x => !x.IsDeleted) == true;
    }

}
