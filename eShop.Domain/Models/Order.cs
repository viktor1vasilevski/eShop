using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;

namespace eShop.Domain.Models;

public class Order : AuditableBaseEntity
{
    public Guid UserId { get; private set; }
    public virtual User? User { get; private set; }

    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItem> _orderItems = [];
    public virtual IReadOnlyCollection<OrderItem>? OrderItems => _orderItems.AsReadOnly();



    private Order() { }

    public static Order Create(Guid userId)
    {
        DomainValidatorHelper.ThrowIfEmptyGuid(userId, nameof(userId));

        return new Order
        {
            UserId = userId,
            Status = OrderStatus.Paid
        };
    }

    public void AddOrderItem(OrderItem item)
    {
        if (item == null) throw new DomainException("OrderItem cannot be null.");
        _orderItems.Add(item);
    }

    public void TotalValue(decimal total)
    {
        TotalAmount = total;
    }
}
