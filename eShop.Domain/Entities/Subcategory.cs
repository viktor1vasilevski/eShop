using eShop.Domain.Entities.Base;
using eShop.Domain.Exceptions;

namespace eShop.Domain.Entities;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; private set; }
    public Guid CategoryId { get; private set; }

    public virtual Category? Category { get; set; }
    public virtual ICollection<Product>? Products { get; set; }




    private Subcategory() { }

    public static Subcategory CreateNew(Guid categoryId, string name)
    {
        if (categoryId == Guid.Empty)
            throw new DomainValidationException("Subcategory Id cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Subcategory name cannot be empty.");

        return new Subcategory
        {
            Id = Guid.NewGuid(),
            Name = name,
            CategoryId = categoryId,
        };
    }


}
