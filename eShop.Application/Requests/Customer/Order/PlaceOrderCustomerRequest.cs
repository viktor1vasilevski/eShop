using eShop.Application.Requests.Customer.OrderItem;

namespace eShop.Application.Requests.Customer.Order;

public class PlaceOrderCustomerRequest
{
    public decimal TotalAmount { get; set; }
    public List<OrderItemCustomerDto> Items { get; set; } = [];
}
