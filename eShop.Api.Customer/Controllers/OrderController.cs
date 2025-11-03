using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses.Customer.Order;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
public class OrderController(IOrderCustomerService _orderCustomerService) : BaseController
{


    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<List<OrderDetailsCustomerResponse>>>> GetOrdersForUser([FromRoute] Guid userId)
    {
        var response = await _orderCustomerService.GetOrdersForUserIdAsync(userId);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDetailsCustomerResponse>>> PlaceOrderAsync([FromBody] PlaceOrderCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _orderCustomerService.PlaceOrderAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
