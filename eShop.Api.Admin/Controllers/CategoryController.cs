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
public class CategoryController(ICategoryAdminService _categoryAdminService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CategoryAdminResponse>>> Get([FromQuery] CategoryAdminRequest request)
    {
        var response = await _categoryAdminService.GetCategoriesAsync(request);
        return HandleResponsee(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminResponse>>> GetById([FromRoute] Guid id)
    {
        var response = await _categoryAdminService.GetCategoryByIdAsync(id);
        return HandleResponsee(response);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminResponse>>> Create([FromBody] CreateCategoryAdminRequest request)
    {
        var response = await _categoryAdminService.CreateCategoryAsync(request);
        return HandleResponsee(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminResponse>>> Update([FromRoute] Guid id, [FromBody] UpdateCategoryAdminRequest request)
    {
        var response = await _categoryAdminService.UpdateCategoryAsync(id, request);
        return HandleResponsee(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<ActionResult<ApiResponse<CategoryEditAdminResponse>>> GetCategoryForEdit(Guid id)
    {
        var response = await _categoryAdminService.GetCategoryForEditAsync(id);
        return HandleResponsee(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDetailsAdminResponse>>> Delete([FromRoute] Guid id)
    {
        var response = await _categoryAdminService.DeleteCategoryAsync(id);
        return HandleResponsee(response);
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<CategoryEditAdminResponse>>> GetCategoryTree()
    {
        var response = await _categoryAdminService.GetCategoryTreeAsync();
        return HandleResponsee(response);
    }
}
