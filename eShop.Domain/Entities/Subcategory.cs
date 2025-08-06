using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Subcategory : AuditableBaseEntity
{
    public string Name { get; set; }
    public Guid CategoryId { get; set; }

    public virtual Category? Category { get; set; }
}
