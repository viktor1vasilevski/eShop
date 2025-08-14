using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; }

    public virtual ICollection<Subcategory>? Subcategories { get; private set; }



    private Category() { }

    public static Category CreateNew(string name)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void Update(string name)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void SoftDelete() => IsDeleted = true;

    public bool HasRelatedSubcategoriesOrProducts()
    {
        return Subcategories?.Any(x => !x.IsDeleted) == true || Subcategories?.FirstOrDefault()?.HasRelatedProducts() == true;
    }

}
