using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses.Customer.Basket;
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

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCustomerRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<BasketCustomerDto>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _orderCustomerService.PlaceOrderAsync(userId.Value, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetOrdersForUser(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<BasketCustomerDto>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _orderCustomerService.GetOrdersForUserIdAsync(userId.Value, cancellationToken);
        return HandleResponse(response);
    }
}
