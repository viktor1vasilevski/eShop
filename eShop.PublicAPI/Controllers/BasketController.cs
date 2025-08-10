using eShop.Application.Interfaces;
using eShop.Application.Requests.Basket;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BasketController(IBasketService _basketService) : BaseController
{

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetByUserIdAsync([FromRoute] Guid userId)
    {
        var response = await _basketService.GetBasketByUserIdAsync(userId);
        return HandleResponse(response);
    }

    [HttpPost("{userId}/merge")]
    public async Task<IActionResult> MergeItemsAsync([FromRoute] Guid userId, [FromBody] List<BasketRequest> request)
    {
        var response = await _basketService.MergeItemsAsync(userId, request);
        return HandleResponse(response);
    }

    [HttpPatch("{userId}/items/{productId}")]
    public async Task<IActionResult> UpdateItemQuantityAsync(Guid userId, Guid productId, [FromBody] UpdateQuantityRequest request)
    {
        var response = await _basketService.UpdateItemQuantityAsync(userId, productId, request.Quantity);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items")]
    public async Task<IActionResult> ClearItemsAsync([FromRoute] Guid userId)
    {
        var response = await _basketService.ClearBasketItemsForUserAsync(userId);
        return HandleResponse(response);
    }

    [HttpDelete("{userId}/items/{productId}")]
    public async Task<IActionResult> RemoveItemAsync([FromRoute] Guid userId, [FromRoute] Guid productId)
    {
        return Ok();
    }
}
