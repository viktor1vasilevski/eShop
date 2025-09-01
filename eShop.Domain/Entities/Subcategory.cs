using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;

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

    public static Subcategory Create(Guid categoryId, string name)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new Subcategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            CategoryId = categoryId,
            IsDeleted = false,
        };
    }

    public void Update(Guid categoryId, string name)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        CategoryId = categoryId;
        Name = name;
    }

    public void SoftDelete() => IsDeleted = true;

    public bool HasRelatedProducts()
    {
        return _products?.Any(x => !x.IsDeleted) == true;
    }

}
