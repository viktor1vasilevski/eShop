using eShop.Application.Interfaces;
using eShop.Application.Requests.Order;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrderController(IOrderService _orderService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] OrderRequest request)
    {
        var response = _orderService.GetOrders(request);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrderAsync([FromBody] PlaceOrderRequest request)
    {
        var response = await _orderService.PlaceOrderAsync(request);
        return HandleResponse(response);
    }


    [HttpGet("{userId}")]
    public IActionResult GetOrdersForUser([FromRoute] Guid userId)
    {
        var response = _orderService.GetOrdersForUserId(userId);
        return HandleResponse(response);
    }
}
