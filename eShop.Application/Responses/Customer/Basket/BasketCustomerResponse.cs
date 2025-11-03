using eShop.Application.DTOs.Customer.BasketItem;

namespace eShop.Application.Responses.Customer.Basket;

public class BasketCustomerResponse
{
    public List<BasketItemCustomerDto> Items { get; set; } = [];
}
