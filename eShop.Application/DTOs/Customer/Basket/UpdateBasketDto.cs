namespace eShop.Application.DTOs.Customer.Basket;

public class UpdateBasketDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
