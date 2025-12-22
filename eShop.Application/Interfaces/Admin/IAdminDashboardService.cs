using eShop.Application.Responses.Admin.Dashboard;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IAdminDashboardService
{
    public Task<ApiResponse<OrdersTodayDto>> GetOrdersTodayAsync(CancellationToken cancellationToken = default);
    public Task<ApiResponse<RevenueTodayDto>> GetRevenueTodayAsync(CancellationToken cancellationToken = default);
    public Task<ApiResponse<TotalCustomersDto>> GetTotalCustomersAsync(CancellationToken cancellationToken = default);
}
