using Azure;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Responses.Admin.Dashboard;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class DashboardController(IAdminDashboardService _adminDashboardService) : BaseController
    {

        [HttpGet("orders-today")]
        public async Task<ActionResult<ApiResponse<OrdersTodayDto>>> GetOrdersToday(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetOrdersTodayAsync(ct);
            return HandleResponse(response);
        }

        [HttpGet("revenue-today")]
        public async Task<ActionResult<ApiResponse<RevenueTodayDto>>> GetRevenueToday(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetRevenueTodayAsync(ct);
            return HandleResponse(response);
        }

        [HttpGet("total-customers")]
        public async Task<ActionResult<ApiResponse<TotalCustomersDto>>> GetTotalCustomers(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetTotalCustomersAsync(ct);
            return HandleResponse(response);
        }
    }
}
