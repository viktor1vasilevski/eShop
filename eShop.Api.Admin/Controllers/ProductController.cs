using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ProductController(IProductAdminService _productAdminService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductAdminResponse>>> Get([FromQuery] ProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.GetProductsAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailsAdminResponse>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.GetProductByIdAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductAdminResponse>>> Create([FromBody] CreateProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.CreateProductAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductAdminResponse>>> Update([FromRoute] Guid id, [FromBody] UpdateProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.UpdateProductAsync(id, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<ActionResult<ApiResponse<ProductEditAdminResponse>>> GetProductForEdit(Guid id, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.GetProductForEditAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<ProductAdminResponse>>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.DeleteProductAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("generate")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateDescription([FromQuery] GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var response = await _productAdminService.GenerateAIProductDescriptionAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
