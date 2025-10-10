namespace eShop.Application.Responses.Customer.OrderItem;

public class OrderItemCustomerDto
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Image { get; set; } = string.Empty;
}
