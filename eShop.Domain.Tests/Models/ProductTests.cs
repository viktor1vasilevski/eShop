using eShop.Domain.Exceptions;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.Models;

public class ProductTests
{
    private static Product CreateTestProduct(string name = "Laptop", int quantity = 10) =>
        Product.Create(name, "Description", 99.99m, quantity, Guid.NewGuid(), Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg"));

    [Fact]
    public void Create_ValidData_SetsPropertiesCorrectly()
    {
        var categoryId = Guid.NewGuid();
        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");

        var product = Product.Create("Laptop", "A laptop", 999m, 5, categoryId, image);

        Assert.Equal("Laptop", product.Name.Value);
        Assert.Equal("A laptop", product.Description.Value);
        Assert.Equal(999m, product.UnitPrice.Value);
        Assert.Equal(5, product.UnitQuantity.Value);
        Assert.Equal(categoryId, product.CategoryId);
        Assert.False(product.IsDeleted);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Fact]
    public void Create_EmptyCategoryId_ThrowsDomainValidationException()
    {
        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        Assert.Throws<DomainValidationException>(() =>
            Product.Create("Laptop", "desc", 10m, 1, Guid.Empty, image));
    }

    [Fact]
    public void SoftDelete_SetsIsDeletedToTrue()
    {
        var product = CreateTestProduct();
        product.SoftDelete();
        Assert.True(product.IsDeleted);
    }

    [Fact]
    public void SubtractQuantity_ValidAmount_ReducesQuantity()
    {
        var product = CreateTestProduct(quantity: 10);
        product.SubtractQuantity(3);
        Assert.Equal(7, product.UnitQuantity.Value);
    }

    [Fact]
    public void SubtractQuantity_MoreThanAvailable_ThrowsDomainValidationException()
    {
        var product = CreateTestProduct(quantity: 5);
        Assert.Throws<DomainValidationException>(() => product.SubtractQuantity(10));
    }

    [Fact]
    public void Update_DoesNotResetIsDeleted()
    {
        var product = CreateTestProduct();
        product.SoftDelete();

        var image = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        product.Update("New Name", "New desc", 10m, 5, Guid.NewGuid(), image);

        Assert.True(product.IsDeleted);
    }

    [Fact]
    public void Update_NullImage_DoesNotReplaceExistingImage()
    {
        var originalImage = Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg");
        var product = Product.Create("Laptop", "desc", 10m, 5, Guid.NewGuid(), originalImage);

        product.Update("New Name", "desc", 10m, 5, Guid.NewGuid(), null!);

        Assert.Equal(originalImage, product.Image);
    }
}
