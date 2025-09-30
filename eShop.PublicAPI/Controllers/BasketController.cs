using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Basket;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BasketController(IBasketCustomerService _basketCustomerService) : BaseController
{

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserIdAsync([FromRoute] Guid userId)
    {
        var response = await _basketCustomerService.GetBasketByUserIdAsync(userId);
        return HandleResponse(response);
    }

    [HttpPost("{userId}/merge")]
    public async Task<IActionResult> UpdateUserBasketAsync([FromRoute] Guid userId, [FromBody] UpdateBasketCustomerRequest request)
    {
        var response = await _basketCustomerService.UpdateUserBasketAsync(userId, request);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items")]
    public async Task<IActionResult> ClearItemsAsync([FromRoute] Guid userId)
    {
        var response = await _basketCustomerService.ClearBasketItemsForUserAsync(userId);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<IActionResult> RemoveItemAsync([FromRoute] Guid userId, [FromRoute] Guid productId)
    {
        var response = await _basketCustomerService.RemoveItemAsync(userId, productId);
        return HandleResponse(response);
    }
}
