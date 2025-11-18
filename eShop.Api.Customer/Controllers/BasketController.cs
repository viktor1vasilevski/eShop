using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
public class BasketController(IBasketCustomerService _basketCustomerService) : BaseController
{

    [HttpGet("{userId}")]
    public async Task<ActionResult<ApiResponse<BasketCustomerDto>>> GetByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var response = await _basketCustomerService.GetBasketByUserIdAsync(userId, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("{userId}/merge")]
    public async Task<ActionResult<ApiResponse<BasketCustomerDto>>> UpdateUserBasket([FromRoute] Guid userId, [FromBody] UpdateBasketCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _basketCustomerService.UpdateUserBasketAsync(userId, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items")]
    public async Task<ActionResult<ApiResponse<BasketCustomerDto>>> ClearItems([FromRoute] Guid userId, CancellationToken cancellationToken)
    {
        var response = await _basketCustomerService.ClearBasketItemsForUserAsync(userId, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<ActionResult<ApiResponse<BasketCustomerDto>>> RemoveItem([FromRoute] Guid userId, [FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var response = await _basketCustomerService.RemoveItemAsync(userId, productId, cancellationToken);
        return HandleResponse(response);
    }
}
