using eShop.Application.Interfaces;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Requests.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
public class OrderController(IOrderService _orderService, IOrderCustomerService _orderCustomerService) : BaseController
{

    [HttpPost]
    public async Task<IActionResult> PlaceOrderAsync([FromBody] PlaceOrderRequest request)
    {
        var response = await _orderCustomerService.PlaceOrderAsync(request);
        return HandleResponse(response);
    }

    [HttpGet("{userId}")]
    public IActionResult GetOrdersForUser([FromRoute] Guid userId)
    {
        var response = _orderService.GetOrdersForUserId(userId);
        return HandleResponse(response);
    }


    //[HttpGet("{userId}")]
    //public IActionResult GetOrdersForUser([FromRoute] Guid userId, [FromBody] OrderRequest request)
    //{
    //    var response = _orderService.GetOrdersForUserId(userId, request);
    //    return HandleResponse(response);
    //}
}
