namespace eShop.Application.Responses.Customer.BasketItem;

public class BasketItemCustomerDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public int UnitQuantity { get; set; }
    public string? Image { get; set; }
}
