using eShop.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService _categoryService) : BaseController
    {

        [HttpGet("CategoriesWithSubcategoriesForMenu")]
        public IActionResult GetCategoriesWithSubcategories()
        {
            var response = _categoryService.GetCategoriesWithSubcategoriesForMenu();
            return HandleResponse(response);
        }
    }
}
