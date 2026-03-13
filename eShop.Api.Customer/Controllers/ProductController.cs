using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(ICustomerProductService _customerProductService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] ProductCustomerRequest request)
    {
        var response = await _customerProductService.GetProductsAsync(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var response = await _customerProductService.GetProductByIdAsync(id, userId, cancellationToken);
        return HandleResponse(response);
    }
}
