using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses.Customer.Order;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface IOrderCustomerService
{
    Task<ApiResponse<OrderDetailsCustomerDto>> PlaceOrderAsync(PlaceOrderCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<OrderDetailsCustomerDto>>> GetOrdersForUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
