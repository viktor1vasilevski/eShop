using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObject;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Models;

public class Category : AuditableBaseEntity
{
    public CategoryName Name { get; private set; }
    public Image Image { get; private set; } = null!;
    public bool IsDeleted { get; private set; }


    public Guid? ParentCategoryId { get; private set; }
    public virtual Category? ParentCategory { get; private set; }


    private readonly List<Category> _children = [];
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();


    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();





    private Category() { }

    public static Category Create(string name, Image image, Guid? parentCategoryId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(parentCategoryId, nameof(parentCategoryId));

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = CategoryName.Create(name),
            Image = image,
            ParentCategoryId = parentCategoryId,
            IsDeleted = false
        };
    }

    public void Update(string name, Image image, Guid? parentCategoryId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(parentCategoryId, nameof(parentCategoryId));

        ParentCategoryId = parentCategoryId;
        if (image != null)
        {
            Image = image;
        }
    }

    public void SoftDelete() => IsDeleted = true;

    public static void SoftDeleteRange(IEnumerable<Category> categories)
    {
        if (categories is null) return;

        foreach (var category in categories)
        {
            category.SoftDelete();
        }
    }

    public record CategoryNode(Guid Id, Guid? ParentCategoryId);

    public static List<Guid> GetDescendantIds(IEnumerable<CategoryNode> allCategories, Guid rootId)
    {
        var result = new List<Guid> { rootId };
        var lookup = allCategories.ToLookup(c => c.ParentCategoryId);

        void Traverse(Guid parentId)
        {
            foreach (var child in lookup[parentId])
            {
                result.Add(child.Id);
                Traverse(child.Id);
            }
        }

        Traverse(rootId);
        return result;
    }

    public record CategoryPathItem(Guid Id, string Name);

    public static List<CategoryPathItem> BuildPath(Guid id, Dictionary<Guid, Category> lookup)
    {
        var result = new List<CategoryPathItem>();
        var currentId = id;

        while (lookup.TryGetValue(currentId, out var current))
        {
            result.Insert(0, new CategoryPathItem(current.Id, current.Name.Value));

            if (current.ParentCategoryId == null)
                break;

            currentId = current.ParentCategoryId.Value;
        }

        return result;
    }

}
