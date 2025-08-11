using eShop.Application.DTOs.Order;
using eShop.Application.Requests.Order;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IOrderService
{
    ApiResponse<List<OrderDTO>> GetOrders(OrderRequest request);
    Task<ApiResponse<OrderDTO>> PlaceOrderAsync(PlaceOrderRequest request);
}
