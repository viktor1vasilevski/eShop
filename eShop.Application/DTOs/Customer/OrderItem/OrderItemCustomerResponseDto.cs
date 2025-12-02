namespace eShop.Application.DTOs.Customer.OrderItem;

public class OrderItemCustomerResponseDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Image { get; set; } = string.Empty;
}
