using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Entities;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Image Image { get; private set; } = null!;
    public bool IsDeleted { get; private set; }

    public Guid CategoryId { get; private set; }
    public virtual Category? Category { get; set; }

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product>? Products => _products.AsReadOnly();

    private Subcategory() { }

    public static Subcategory Create(string name, Guid categoryId, Image? image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));

        return new Subcategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            Image = image,
            CategoryId = categoryId,
            IsDeleted = false
        };
    }

    public void Update(string name, Guid categoryId, Image image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));

        Name = name;
        CategoryId = categoryId;
        Image = image;
    }

    public void SoftDelete() => IsDeleted = true;

    public bool HasRelatedProducts()
    {
        return _products.Any(x => !x.IsDeleted);
    }
}
