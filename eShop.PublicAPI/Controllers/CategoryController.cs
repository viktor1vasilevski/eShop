using eShop.Application.Interfaces.Category;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryCustomerService _categoryCustomerService) : BaseController
{

    [HttpGet("CategoryTreeForMenu")]
    public async Task<IActionResult> GetCategoryTreeForMenu()
    {
        var response = await _categoryCustomerService.GetCategoryTreeForMenuAsync();
        return HandleResponse(response);
    }
}
