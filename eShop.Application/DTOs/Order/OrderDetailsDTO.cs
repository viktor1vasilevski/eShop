using eShop.Application.DTOs.OrderItem;

namespace eShop.Application.DTOs.Order;

public class OrderDetailsDTO
{
    public List<OrderItemDTO>? Items { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderCreatedOn { get; set; }

}
