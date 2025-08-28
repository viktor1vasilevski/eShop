using eShop.Application.DTOs.OrderItem;

namespace eShop.Application.DTOs.Order;

public class OrderDetailsDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public List<OrderItemDTO>? Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderCreatedOn { get; set; }

}
