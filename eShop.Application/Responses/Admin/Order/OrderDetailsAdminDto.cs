using eShop.Application.Responses.Admin.OrderItem;

namespace eShop.Application.Responses.Admin.Order;

public class OrderDetailsAdminDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public List<OrderItemAdminDto>? Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderCreatedOn { get; set; }
}
