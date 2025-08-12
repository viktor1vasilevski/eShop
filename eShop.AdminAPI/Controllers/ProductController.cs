using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Product;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class ProductController(IProductService _productService) : BaseController
    {


        [HttpGet]
        public IActionResult Get([FromQuery] ProductRequest request)
        {
            var response = _productService.GetProducts(request);
            return HandleResponse(response);
        }


        [HttpPost]
        public IActionResult Create([FromBody] CreateUpdateProductRequest request)
        {
            var response = _productService.CreateProduct(request);
            if (response.NotificationType == NotificationType.Created && response.Data?.Id != null)
            {
                var locationUri = Url.Action(nameof(GetById), "Product", new { id = response.Data.Id }, Request.Scheme);
                response.Location = locationUri;
            }
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] Guid id, [FromQuery] Guid? userId = null)
        {
            var response = _productService.GetProductById(id, userId);
            return HandleResponse(response);
        }


        [HttpPut("{id}")]
        public IActionResult Update([FromRoute] Guid id, [FromBody] CreateUpdateProductRequest request)
        {
            var response = _productService.UpdateProduct(id, request);
            return HandleResponse(response);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete([FromRoute] Guid id)
        {
            var response = _productService.DeleteProduct(id);
            return HandleResponse(response);
        }
    }
}
