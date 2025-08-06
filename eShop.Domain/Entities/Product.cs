using eShop.Domain.Entities.Base;

namespace eShop.Domain.Entities;

public class Product : AuditableBaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitQuantity { get; set; }
    public byte[] Image { get; set; }
    public string ImageType { get; set; }

    public Guid SubcategoryId { get; set; }
    public virtual Subcategory? Subcategory { get; set; }
}
