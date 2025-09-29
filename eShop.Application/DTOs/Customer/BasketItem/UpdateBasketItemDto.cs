namespace eShop.Application.DTOs.Customer.BasketItem;

public class UpdateBasketItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
