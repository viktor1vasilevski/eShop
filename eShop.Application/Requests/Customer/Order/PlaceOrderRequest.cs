using eShop.Application.DTOs.Customer.OrderItem;

namespace eShop.Application.Requests.Customer.Order;

public class PlaceOrderRequest
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}
