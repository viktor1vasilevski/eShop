using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GetById([FromRoute] Guid id, [FromQuery] Guid? userId = null)
    {
        //var response = _productService.GetProductById(id, userId);
        //return HandleResponse(response);
        return Ok();
    }
}
