using eShop.Application.Interfaces;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Admin.Product;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductCustomerService _productCustomerService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] ProductAdminRequest request)
    {
        //var response = _productService.GetProducts(request);
        //return HandleResponse(response);
        return Ok();
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] Guid id, [FromQuery] Guid? userId = null)
    {
        //var response = _productService.GetProductById(id, userId);
        //return HandleResponse(response);
        return Ok();
    }
}
