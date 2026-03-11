using eShop.Domain.Exceptions;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;

namespace eShop.Domain.Tests.Models;

public class BasketTests
{
    private static Product CreateTestProduct(int quantity = 10) =>
        Product.Create("Laptop", "desc", 99m, quantity, Guid.NewGuid(), Image.FromBytes(new byte[] { 1, 2, 3 }, "jpeg"));

    [Fact]
    public void CreateNew_ValidUserId_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var basket = Basket.CreateNew(userId);

        Assert.Equal(userId, basket.UserId);
        Assert.NotEqual(Guid.Empty, basket.Id);
        Assert.Empty(basket.BasketItems!);
    }

    [Fact]
    public void CreateNew_EmptyUserId_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => Basket.CreateNew(Guid.Empty));
    }

    [Fact]
    public void AddOrUpdateItem_NewProduct_AddsItem()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        var product = CreateTestProduct();

        basket.AddOrUpdateItem(product, 2);

        Assert.Single(basket.BasketItems!);
        Assert.Equal(2, basket.BasketItems!.First().UnitQuantity.Value);
    }

    [Fact]
    public void AddOrUpdateItem_ExistingProduct_UpdatesQuantity()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        var product = CreateTestProduct(quantity: 10);

        basket.AddOrUpdateItem(product, 2);
        basket.AddOrUpdateItem(product, 3);

        Assert.Single(basket.BasketItems!);
        Assert.Equal(5, basket.BasketItems!.First().UnitQuantity.Value);
    }

    [Fact]
    public void AddOrUpdateItem_QuantityExceedsStock_CapsAtStockQuantity()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        var product = CreateTestProduct(quantity: 3);

        basket.AddOrUpdateItem(product, 10);

        Assert.Equal(3, basket.BasketItems!.First().UnitQuantity.Value);
    }

    [Fact]
    public void AddOrUpdateItem_NullProduct_ThrowsDomainException()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        Assert.Throws<DomainException>(() => basket.AddOrUpdateItem(null!, 1));
    }

    [Fact]
    public void RemoveItem_ExistingProduct_RemovesIt()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        var product = CreateTestProduct();

        basket.AddOrUpdateItem(product, 1);
        basket.RemoveItem(product.Id);

        Assert.Empty(basket.BasketItems!);
    }

    [Fact]
    public void RemoveItem_NonExistentProduct_DoesNotThrow()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        var exception = Record.Exception(() => basket.RemoveItem(Guid.NewGuid()));
        Assert.Null(exception);
    }

    [Fact]
    public void ClearItems_RemovesAllItems()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        basket.AddOrUpdateItem(CreateTestProduct(), 1);
        basket.AddOrUpdateItem(CreateTestProduct(), 2);

        basket.ClearItems();

        Assert.Empty(basket.BasketItems!);
    }

    [Fact]
    public void UpdateItemQuantity_ItemNotFound_ThrowsDomainException()
    {
        var basket = Basket.CreateNew(Guid.NewGuid());
        Assert.Throws<DomainException>(() => basket.UpdateItemQuantity(Guid.NewGuid(), 5));
    }
}
