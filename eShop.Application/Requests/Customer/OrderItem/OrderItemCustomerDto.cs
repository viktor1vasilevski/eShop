namespace eShop.Application.Requests.Customer.OrderItem;

public class OrderItemCustomerDto
{
    public string ProductName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
