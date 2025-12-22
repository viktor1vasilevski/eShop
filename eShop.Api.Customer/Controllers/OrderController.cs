using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
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
public class OrderController(ICustomerOrderService _orderCustomerService) : BaseController
{

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDetailsCustomerDto>>> PlaceOrder([FromBody] PlaceOrderCustomerRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<OrderDetailsCustomerDto>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _orderCustomerService.PlaceOrderAsync(userId.Value, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<OrderDetailsCustomerDto>>>> GetOrdersForUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<List<OrderDetailsCustomerDto>>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _orderCustomerService.GetOrdersForUserIdAsync(userId.Value, cancellationToken);
        return HandleResponse(response);
    }
}
