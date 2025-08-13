using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public Guid CategoryId { get; private set; }
    public virtual Category? Category { get; set; }

    public virtual ICollection<Product>? Products { get; private set; }




    private Subcategory() { }

    public static Subcategory CreateNew(Guid categoryId, string name)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(categoryId, nameof(categoryId));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new Subcategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            CategoryId = categoryId
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
        return Products?.Any(x => !x.IsDeleted) == true;
    }

}
