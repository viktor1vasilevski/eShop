using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryAdminService _categoryAdminService) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CategoryAdminRequest request)
    {
        var response = await _categoryAdminService.GetCategories(request);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
    {
        var response = await _categoryAdminService.CreateCategory(request);
        return HandleResponse(response);
    }
}
