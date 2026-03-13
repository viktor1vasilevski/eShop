namespace eShop.Domain.Models.Base;

public abstract class AuditableBaseEntity : BaseEntity
{
    public virtual string CreatedBy { get; set; } = string.Empty;
    public virtual DateTime Created { get; set; }
    public virtual string? LastModifiedBy { get; set; }
    public virtual DateTime? LastModified { get; set; }
}
