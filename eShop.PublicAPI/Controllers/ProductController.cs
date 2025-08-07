using eShop.Application.Interfaces;
using eShop.Application.Requests.Product;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(IProductService _productService) : BaseController
    {

        [HttpGet]
        public IActionResult Get([FromQuery] ProductRequest request)
        {
            var response = _productService.GetProducts(request);
            return HandleResponse(response);
        }
    }
}
