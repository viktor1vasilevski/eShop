using eShop.Application.Responses.Admin.Dashboard;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IAdminDashboardService
{
    public Task<Result<OrdersTodayDto>> GetOrdersTodayAsync(CancellationToken cancellationToken = default);
    public Task<Result<RevenueTodayDto>> GetRevenueTodayAsync(CancellationToken cancellationToken = default);
    public Task<Result<TotalCustomersDto>> GetTotalCustomersAsync(CancellationToken cancellationToken = default);
}
