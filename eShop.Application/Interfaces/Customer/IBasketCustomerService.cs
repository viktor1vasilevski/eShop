using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface IBasketCustomerService
{
    Task<ApiResponse<BasketCustomerDto>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerDto>> GetBasketByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerDto>> ClearBasketItemsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<BasketCustomerDto>> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
}
