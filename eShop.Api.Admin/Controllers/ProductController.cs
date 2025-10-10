using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class ProductController(IProductAdminService _productAdminService) : BaseController
    {


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] ProductAdminRequest request, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.GetProductsAsync(request, cancellationToken);
            return HandleResponse(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.GetProductByIdAsync(id, cancellationToken);
            return HandleResponse(response);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductAdminRequest request, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.CreateProductAsync(request, cancellationToken);
            return HandleResponse(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductAdminRequest request, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.UpdateProductAsync(id, request, cancellationToken);
            return HandleResponse(response);
        }

        [HttpGet("{id}/edit")]
        public async Task<IActionResult> GetProductForEdit(Guid id, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.GetProductForEditAsync(id, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.DeleteProductAsync(id, cancellationToken);
            return HandleResponse(response);
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GenerateDescription([FromQuery] GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
        {
            var response = await _productAdminService.GenerateAIProductDescriptionAsync(request, cancellationToken);
            return HandleResponse(response);
        }
    }
}
