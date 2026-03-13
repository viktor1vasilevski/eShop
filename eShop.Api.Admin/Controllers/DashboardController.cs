using eShop.Application.Interfaces.Admin;
using eShop.Application.Responses.Admin.Dashboard;
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
        public async Task<ActionResult> GetOrdersToday(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetOrdersTodayAsync(ct);
            return HandleResponse(response);
        }

        [HttpGet("revenue-today")]
        public async Task<ActionResult> GetRevenueToday(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetRevenueTodayAsync(ct);
            return HandleResponse(response);
        }

        [HttpGet("total-customers")]
        public async Task<ActionResult> GetTotalCustomers(CancellationToken ct)
        {
            var response = await _adminDashboardService.GetTotalCustomersAsync(ct);
            return HandleResponse(response);
        }
    }
}
