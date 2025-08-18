using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Category;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class CategoryController(ICategoryService _categoryService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] CategoryRequest request)
    {
        var response = _categoryService.GetCategories(request);
        return HandleResponse(response);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUpdateCategoryRequest request)
    {
        var response = _categoryService.CreateCategory(request);
        if (response.NotificationType == ResponseStatus.Created && response.Data?.Id != null)
        {
            var locationUri = Url.Action(nameof(GetById), "Category", new { id = response.Data.Id }, Request.Scheme);
            response.Location = locationUri;
        }

        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] CreateUpdateCategoryRequest request)
    {
        var response = _categoryService.UpdateCategory(id, request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var response = _categoryService.GetCategoryById(id);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var response = _categoryService.DeleteCategory(id);
        return HandleResponse(response);
    }

    [HttpGet("GetCategoriesDropdownList")]
    public async Task<IActionResult> GetCategoriesDropdownListAsync()
    {
        var response = await _categoryService.GetCategoriesDropdownListAsync();
        return HandleResponse(response);
    }
}
