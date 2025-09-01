using eShop.Application.DTOs.Order;
using eShop.Application.Requests.Order;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IOrderService
{
    ApiResponse<List<OrderDetailsDTO>> GetOrders(OrderRequest request);
    ApiResponse<List<OrderDetailsDTO>> GetOrdersForUserId(Guid userId);
    Task<ApiResponse<OrderDetailsDTO>> PlaceOrderAsync(PlaceOrderRequest request);
}
