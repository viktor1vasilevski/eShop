namespace eShop.Application.DTOs.Customer.OrderItem;

public class PlaceOrderItemCustomerDto
{
    public string ProductName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
