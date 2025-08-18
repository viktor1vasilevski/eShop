using eShop.Application.Interfaces;
using eShop.Application.Requests.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class OrderController(IOrderService _orderService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] OrderRequest request)
    {
        var response = _orderService.GetOrders(request);
        return HandleResponse(response);
    }


    [HttpGet("{userId}")]
    public IActionResult GetOrdersForUser([FromRoute] Guid userId)
    {
        var response = _orderService.GetOrdersForUserId(userId);
        return HandleResponse(response);
    }
}
