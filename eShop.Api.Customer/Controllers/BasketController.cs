using eShop.Application.Constants.Customer;
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
public class BasketController(ICustomerBasketService _customerBasketService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(Result<BasketCustomerDto>.Unauthorized(CustomerAuthConstants.UserNotAuthenticated));

        var response = await _customerBasketService.GetBasketByUserIdAsync(userId.Value, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("merge")]
    public async Task<ActionResult> UpdateUserBasket([FromBody] UpdateBasketCustomerRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(Result<BasketCustomerDto>.Unauthorized(CustomerAuthConstants.UserNotAuthenticated));

        var response = await _customerBasketService.UpdateUserBasketAsync(userId.Value, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("items")]
    public async Task<ActionResult> ClearItems(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(Result<BasketCustomerDto>.Unauthorized(CustomerAuthConstants.UserNotAuthenticated));

        var response = await _customerBasketService.ClearBasketItemsForUserAsync(userId.Value, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("items/{productId}")]
    public async Task<ActionResult> RemoveItem([FromRoute] Guid productId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(Result<BasketCustomerDto>.Unauthorized(CustomerAuthConstants.UserNotAuthenticated));

        var response = await _customerBasketService.RemoveItemAsync(userId.Value, productId, cancellationToken);
        return HandleResponse(response);
    }
}
