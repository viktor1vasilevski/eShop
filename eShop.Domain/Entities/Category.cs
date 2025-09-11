using eShop.Domain.Entities.Base;
using eShop.Domain.Helpers;
using eShop.Domain.ValueObjects;


namespace eShop.Domain.Entities;

public class Category : AuditableBaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public Image Image { get; private set; } = null!;
    public bool IsDeleted { get; private set; }


    public Guid? ParentCategoryId { get; private set; }
    public virtual Category? ParentCategory { get; private set; }

    private readonly List<Category> _children = new();
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();





    private Category() { }

    public static Category Create(string name, Image image, Guid? parentCategoryId)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(name, nameof(name));
        DomainValidatorHelper.ThrowIfEmptyGuid(parentCategoryId, nameof(parentCategoryId));

        return new Category 
        { 
            Id = Guid.NewGuid(), 
            Name = name, 
            Image = image, 
            ParentCategoryId = parentCategoryId,
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
