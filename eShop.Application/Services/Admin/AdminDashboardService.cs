using eShop.Application.Enums;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Responses.Admin.Dashboard;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;

namespace eShop.Application.Services.Admin;

public class AdminDashboardService(IEfUnitOfWork _uow, IEfRepository<Order> _orderRepository, IEfRepository<User> _userRepository) : IAdminDashboardService
{
    public async Task<ApiResponse<OrdersTodayDto>> GetOrdersTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var (items, _) = await _orderRepository.QueryAsync(
            queryBuilder: q => q.Where(o => o.Created >= today),
            selector: o => o.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        var count = items.Count;

        return new ApiResponse<OrdersTodayDto>
        {
            Status = ResponseStatus.Success,
            Data = new OrdersTodayDto { Count = count }
        };

    }

    public async Task<ApiResponse<RevenueTodayDto>> GetRevenueTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var (items, _) = await _orderRepository.QueryAsync(
            queryBuilder: q => q.Where(o => o.Created >= today),
            selector: o => o.TotalAmount,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        var revenue = items.Sum();

        return new ApiResponse<RevenueTodayDto>
        {
            Status = ResponseStatus.Success,
            Data = new RevenueTodayDto { Amount = revenue }
        };
    }

    public async Task<ApiResponse<TotalCustomersDto>> GetTotalCustomersAsync(CancellationToken cancellationToken = default)
    {
        var (items, _) = await _userRepository.QueryAsync(
            selector: u => u.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        var total = items.Count;

        return new ApiResponse<TotalCustomersDto>
        {
            Status = ResponseStatus.Success,
            Data = new TotalCustomersDto { Count = total }
        };
    }
}
