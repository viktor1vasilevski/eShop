namespace eShop.Application.Requests.Customer.BasketItem;

public class UpdateBasketItemCustomerDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
