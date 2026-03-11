using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Models;

namespace eShop.Domain.Tests.Models;

public class OrderTests
{
    [Fact]
    public void Create_ValidUserId_SetsPropertiesCorrectly()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(userId);

        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Paid, order.Status);
        Assert.Empty(order.OrderItems!);
    }

    [Fact]
    public void Create_EmptyUserId_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => Order.Create(Guid.Empty));
    }

    [Fact]
    public void AddOrderItem_ValidItem_AddsToOrderItems()
    {
        var order = Order.Create(Guid.NewGuid());
        var item = OrderItem.Create(Guid.NewGuid(), 2, 50m);

        order.AddOrderItem(item);

        Assert.Single(order.OrderItems!);
    }

    [Fact]
    public void AddOrderItem_NullItem_ThrowsDomainException()
    {
        var order = Order.Create(Guid.NewGuid());
        Assert.Throws<DomainException>(() => order.AddOrderItem(null!));
    }

    [Fact]
    public void AddOrderItem_MultipleItems_AddsAll()
    {
        var order = Order.Create(Guid.NewGuid());
        order.AddOrderItem(OrderItem.Create(Guid.NewGuid(), 1, 10m));
        order.AddOrderItem(OrderItem.Create(Guid.NewGuid(), 2, 20m));

        Assert.Equal(2, order.OrderItems!.Count);
    }

    [Fact]
    public void TotalValue_SetsTotalAmount()
    {
        var order = Order.Create(Guid.NewGuid());
        order.TotalValue(199.99m);

        Assert.Equal(199.99m, order.TotalAmount);
    }
}
