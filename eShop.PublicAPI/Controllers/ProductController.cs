using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductCustomerService _productCustomerService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] ProductCustomerRequest request)
    {
        var response = _productCustomerService.GetProducts(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        Guid? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && Guid.TryParse(claim.Value, out var parsedId))
            {
                userId = parsedId;
            }
        }

        var response = await _productCustomerService.GetProductById(id, userId);
        return HandleResponse(response);
    }
}
