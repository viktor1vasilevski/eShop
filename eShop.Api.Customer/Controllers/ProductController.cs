using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductCustomerService _productCustomerService) : BaseController
{


    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductCustomerResponse>>>> Get([FromQuery] ProductCustomerRequest request)
    {
        var response = await _productCustomerService.GetProductsAsync(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailsCustomerResponse>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
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

        var response = await _productCustomerService.GetProductByIdAsync(id, userId, cancellationToken);
        return HandleResponse(response);
    }
}
