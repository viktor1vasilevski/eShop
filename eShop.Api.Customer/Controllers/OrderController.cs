using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
public class OrderController(IOrderCustomerService _orderCustomerService) : BaseController
{

    [HttpPost]
    public async Task<IActionResult> PlaceOrderAsync([FromBody] PlaceOrderCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _orderCustomerService.PlaceOrderAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetOrdersForUser([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var response = await _orderCustomerService.GetOrdersForUserIdAsync(userId, cancellationToken);
        return HandleResponse(response);
    }
}
