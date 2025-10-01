using eShop.Application.Requests.Admin.Order;
using eShop.Application.Responses;
using eShop.Application.Responses.Admin.Order;

namespace eShop.Application.Interfaces.Admin;

public interface IOrderAdminService
{
    ApiResponse<List<OrderDetailsAdminDto>> GetOrders(OrderAdminRequest request);
}
