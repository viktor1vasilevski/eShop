namespace eShop.Application.Requests.Customer.OrderItem;

public class OrderItemCustomerDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
