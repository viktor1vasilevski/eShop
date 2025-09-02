using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public byte[] Image { get; private set; } = [];
    public string ImageType { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }


    private readonly List<Subcategory> _subcategories = [];
    public IReadOnlyCollection<Subcategory>? Subcategories => _subcategories.AsReadOnly();



    private Category() { }

    public static Category Create(string name, string? image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var imageBytes = ImageHelper.ConvertBase64ToBytes(image);
        var imageType = ImageHelper.ExtractImageType(image);

        ImageHelper.ValidateImage(imageBytes, imageType);

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            Image = imageBytes,
            ImageType = imageType,
            IsDeleted = false,
        };
    }

    public void Update(string name, string? image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        var imageBytes = ImageHelper.ConvertBase64ToBytes(image);
        var imageType = ImageHelper.ExtractImageType(image);

        ImageHelper.ValidateImage(imageBytes, imageType);

        Name = name;
        Image = imageBytes;
        ImageType = imageType;
    }

    public void SoftDelete() => IsDeleted = true;

    public bool HasRelatedSubcategories()
    {
        return _subcategories.Any(x => !x.IsDeleted);
    }
}
