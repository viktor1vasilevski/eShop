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
public class ProductController(IAdminProductService _adminProductService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ProductAdminDto>>> Get([FromQuery] ProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.GetProductsAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailsAdminDto>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.GetProductByIdAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductAdminDto>>> Create([FromBody] CreateProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.CreateProductAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductAdminDto>>> Update([FromRoute] Guid id, [FromBody] UpdateProductAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.UpdateProductAsync(id, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<ActionResult<ApiResponse<ProductEditAdminDto>>> GetProductForEdit(Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.GetProductForEditAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<ProductAdminDto>>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.DeleteProductAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("generate")]
    public async Task<ActionResult<ApiResponse<string>>> GenerateDescription([FromQuery] GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminProductService.GenerateAIProductDescriptionAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
