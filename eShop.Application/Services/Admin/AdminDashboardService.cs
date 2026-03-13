using eShop.Application.Interfaces.Admin;
using eShop.Application.Responses.Admin.Dashboard;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;

namespace eShop.Application.Services.Admin;

public class AdminDashboardService(IEfRepository<Order> _orderRepository, IEfRepository<User> _userRepository) : IAdminDashboardService
{
    public async Task<Result<OrdersTodayDto>> GetOrdersTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var (items, _) = await _orderRepository.QueryAsync(
            queryBuilder: q => q.Where(o => o.Created >= today),
            selector: o => o.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return Result<OrdersTodayDto>.Success(new OrdersTodayDto { Count = items.Count });
    }

    public async Task<Result<RevenueTodayDto>> GetRevenueTodayAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var (items, _) = await _orderRepository.QueryAsync(
            queryBuilder: q => q.Where(o => o.Created >= today),
            selector: o => o.TotalAmount,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return Result<RevenueTodayDto>.Success(new RevenueTodayDto { Amount = items.Sum() });
    }

    public async Task<Result<TotalCustomersDto>> GetTotalCustomersAsync(CancellationToken cancellationToken = default)
    {
        var (items, _) = await _userRepository.QueryAsync(
            selector: u => u.Id,
            asNoTracking: true,
            cancellationToken: cancellationToken);

        return Result<TotalCustomersDto>.Success(new TotalCustomersDto { Count = items.Count });
    }
}
