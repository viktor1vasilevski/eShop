namespace eShop.Application.Requests.Basket;

public class BasketRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
