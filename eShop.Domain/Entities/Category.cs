using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;

namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; private set; }

    public virtual ICollection<Subcategory>? Subcategories { get; set; }



    private Category() { }

    public static Category CreateNew(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Category name cannot be empty.");

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
        };
    }
}
