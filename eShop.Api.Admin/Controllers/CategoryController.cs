using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
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
    public async Task<IActionResult> Get([FromQuery] CategoryAdminRequest request)
    {
        var response = await _categoryAdminService.GetCategoriesAsync(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var response = await _categoryAdminService.GetCategoryByIdAsync(id);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var response = await _categoryAdminService.CreateCategoryAsync(request);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var response = await _categoryAdminService.UpdateCategoryAsync(id, request);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> GetCategoryForEdit(Guid id)
    {
        var response = await _categoryAdminService.GetCategoryForEditAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var response = await _categoryAdminService.DeleteCategoryAsync(id);
        return HandleResponse(response);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        var response = await _categoryAdminService.GetCategoryTreeAsync();
        return HandleResponse(response);
    }
}
