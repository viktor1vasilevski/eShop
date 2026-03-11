using eShop.Domain.Exceptions;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.Models;

public class CategoryTests
{
    private static Image TestImage() => Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");

    [Fact]
    public void Create_ValidData_SetsPropertiesCorrectly()
    {
        var category = Category.Create("Electronics", TestImage(), null);

        Assert.Equal("Electronics", category.Name.Value);
        Assert.False(category.IsDeleted);
        Assert.Null(category.ParentCategoryId);
        Assert.NotEqual(Guid.Empty, category.Id);
    }

    [Fact]
    public void Create_EmptyGuidAsParent_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() =>
            Category.Create("Electronics", TestImage(), Guid.Empty));
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedToTrue()
    {
        var category = Category.Create("Electronics", TestImage(), null);
        category.SoftDelete();
        Assert.True(category.IsDeleted);
    }

    [Fact]
    public void SoftDeleteRange_MarksAllAsDeleted()
    {
        var categories = new List<Category>
        {
            Category.Create("Electronics", TestImage(), null),
            Category.Create("Clothing", TestImage(), null)
        };

        Category.SoftDeleteRange(categories);

        Assert.All(categories, c => Assert.True(c.IsDeleted));
    }

    [Fact]
    public void SoftDeleteRange_NullInput_DoesNotThrow()
    {
        var exception = Record.Exception(() => Category.SoftDeleteRange(null!));
        Assert.Null(exception);
    }

    [Fact]
    public void GetDescendantIds_ReturnsRootAndAllDescendants()
    {
        var root = Guid.NewGuid();
        var child1 = Guid.NewGuid();
        var child2 = Guid.NewGuid();
        var grandchild = Guid.NewGuid();

        var nodes = new List<Category.CategoryNode>
        {
            new(root, null),
            new(child1, root),
            new(child2, root),
            new(grandchild, child1)
        };

        var result = Category.GetDescendantIds(nodes, root);

        Assert.Contains(root, result);
        Assert.Contains(child1, result);
        Assert.Contains(child2, result);
        Assert.Contains(grandchild, result);
        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void GetDescendantIds_LeafNode_ReturnsOnlyItself()
    {
        var id = Guid.NewGuid();
        var nodes = new List<Category.CategoryNode> { new(id, null) };

        var result = Category.GetDescendantIds(nodes, id);

        Assert.Single(result);
        Assert.Contains(id, result);
    }

    [Fact]
    public void BuildPath_SingleCategory_ReturnsOneItem()
    {
        var category = Category.Create("Electronics", TestImage(), null);
        var lookup = new Dictionary<Guid, Category> { { category.Id, category } };

        var path = Category.BuildPath(category.Id, lookup);

        Assert.Single(path);
        Assert.Equal("Electronics", path[0].Name);
    }

    [Fact]
    public void BuildPath_UnknownId_ReturnsEmptyList()
    {
        var path = Category.BuildPath(Guid.NewGuid(), new Dictionary<Guid, Category>());
        Assert.Empty(path);
    }
}
