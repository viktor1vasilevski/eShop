using eShop.Application.Requests.Customer.BasketItem;

namespace eShop.Application.Requests.Customer.Basket;

public class UpdateBasketCustomerRequest
{
    public List<UpdateBasketItemCustomerDto> Items { get; set; }
}
