using eShop.Application.DTOs.Customer.OrderItem;

namespace eShop.Application.Responses.Customer.Order;

public class OrderDetailsCustomerDto
{
    public Guid OrderId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public List<OrderItemCustomerResponseDto> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public DateTime OrderCreatedOn { get; set; }
}
