using eShop.Application.Interfaces.Admin;
using eShop.Domain.Interfaces.EntityFramework;

namespace eShop.Application.Services.Admin;

public class AdminDashboardService(IEfUnitOfWork _uow) : IAdminDashboardService
{
}
