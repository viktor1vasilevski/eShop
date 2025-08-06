using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; set; }
}
