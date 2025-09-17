using eShop.Application.Enums;
using eShop.Application.Interfaces.Category;
using eShop.Application.Requests.Category;
using eShop.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class CategoryController(ICategoryAdminService _categoryAdminService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] CategoryRequest request)
    {
        var response = _categoryAdminService.GetCategories(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var response = await _categoryAdminService.GetCategoryByIdAsync(id);
        return HandleResponse(response);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUpdateCategoryRequest request)
    {
        var response = _categoryAdminService.CreateCategory(request);
        if (response.Status == ResponseStatus.Created && response.Data?.Id != null)
        {
            var locationUri = Url.Action(nameof(GetByIdAsync), nameof(Category), new { id = response.Data.Id }, Request.Scheme);
            response.Location = locationUri;
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] CreateUpdateCategoryRequest request)
    {
        var response = await _categoryAdminService.UpdateCategory(id, request);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> GetCategoryForEdit(Guid id)
    {
        var response = await _categoryAdminService.GetCategoryForEditAsync(id);
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var response = _categoryAdminService.DeleteCategory(id);
        return HandleResponse(response);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        var response = await _categoryAdminService.GetCategoryTreeAsync();
        return HandleResponse(response);
    }
}
