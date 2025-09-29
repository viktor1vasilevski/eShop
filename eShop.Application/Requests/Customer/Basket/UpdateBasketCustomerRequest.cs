using eShop.Application.DTOs.Customer.Basket;
using eShop.Application.DTOs.Customer.BasketItem;

namespace eShop.Application.Requests.Customer.Basket;

public class UpdateBasketCustomerRequest
{
    public List<UpdateBasketItemDto> Items { get; set; }
}
