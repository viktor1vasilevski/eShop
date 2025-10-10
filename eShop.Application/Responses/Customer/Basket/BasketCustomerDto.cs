using eShop.Application.Responses.Customer.BasketItem;

namespace eShop.Application.Responses.Customer.Basket;

public class BasketCustomerDto
{
    public List<BasketItemCustomerDto> Items { get; set; } = [];
}
