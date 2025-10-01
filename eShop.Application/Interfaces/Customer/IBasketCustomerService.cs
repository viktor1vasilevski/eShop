using eShop.Application.DTOs.Basket;
using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces.Customer;

public interface IBasketCustomerService
{
    Task<ApiResponse<BasketDTO>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request);
    Task<ApiResponse<BasketDTO>> GetBasketByUserIdAsync(Guid userId);
    Task<ApiResponse<BasketDTO>> ClearBasketItemsForUserAsync(Guid userId);
    Task<ApiResponse<BasketDTO>> RemoveItemAsync(Guid userId, Guid productId);
}
