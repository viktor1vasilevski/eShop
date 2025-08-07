using eShop.Application.DTOs.Basket;
using eShop.Application.Requests.Basket;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IBasketService
{
    Task<ApiResponse<BasketDTO>> Merge(Guid userId, List<BasketRequest> request);
    Task<ApiResponse<BasketDTO>> GetBasketByUserIdAsync(Guid userId);
    Task<ApiResponse<BasketDTO>> UpdateItemQuantityAsync(Guid userId, Guid productId, int newQuantity);

}
