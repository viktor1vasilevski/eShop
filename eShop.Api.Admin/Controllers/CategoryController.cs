using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class CategoryController(IAdminCategoryService _categoryAdminService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CategoryAdminDto>>> Get([FromQuery] CategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.GetCategoriesAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminDto>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.GetCategoryByIdAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminDto>>> Create([FromBody] CreateCategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.CreateCategoryAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminDto>>> Update([FromRoute] Guid id, [FromBody] UpdateCategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.UpdateCategoryAsync(id, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<ActionResult<ApiResponse<CategoryEditAdminDto>>> GetCategoryForEdit(Guid id, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.GetCategoryForEditAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminDto>>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.DeleteCategoryAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<CategoryEditAdminDto>>> GetCategoryTree(CancellationToken cancellationToken)
    {
        var response = await _categoryAdminService.GetCategoryTreeAsync(cancellationToken);
        return HandleResponse(response);
    }
}
