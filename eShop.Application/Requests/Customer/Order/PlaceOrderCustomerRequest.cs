using eShop.Application.DTOs.Customer.OrderItem;

namespace eShop.Application.Requests.Customer.Order;

public class PlaceOrderCustomerRequest
{
    public decimal TotalAmount { get; set; }
    public List<PlaceOrderItemCustomerDto> Items { get; set; } = [];
}
