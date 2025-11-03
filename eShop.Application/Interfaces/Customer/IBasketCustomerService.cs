using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface IBasketCustomerService
{
    Task<ApiResponse<BasketCustomerResponse>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerResponse>> GetBasketByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerResponse>> ClearBasketItemsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerResponse>> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
