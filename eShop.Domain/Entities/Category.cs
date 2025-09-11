using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;
using eShop.Domain.ValueObjects;


namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Image Image { get; private set; } = null!;
    public bool IsDeleted { get; private set; }




    private Category() { }

    public static Category Create(string name, Image image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        return new Category 
        { 
            Id = Guid.NewGuid(), 
            Name = name, 
            Image = image, 
            IsDeleted = false
        };
    }

    public void Update(string name, Image image)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Name = name;
        Image = image;
    }

    public void SoftDelete() => IsDeleted = true;
}
