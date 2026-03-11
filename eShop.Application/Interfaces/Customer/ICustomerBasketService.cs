using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerBasketService
{
    Task<Result<BasketCustomerDto>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<BasketCustomerDto>> GetBasketByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<BasketCustomerDto>> ClearBasketItemsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<BasketCustomerDto>> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
