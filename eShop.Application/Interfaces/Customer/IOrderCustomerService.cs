using eShop.Application.DTOs.Order;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces.Customer;

public interface IOrderCustomerService
{
    Task<ApiResponse<OrderDetailsDTO>> PlaceOrderAsync(PlaceOrderRequest request);
}
