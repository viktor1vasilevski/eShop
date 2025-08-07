using eShop.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryService _categoryService) : BaseController
    {

        [HttpGet("CategoriesWithSubcategoriesForMenu")]
        public IActionResult GetCategoriesDropdownList()
        {
            var response = _categoryService.GetCategoriesWithSubcategoriesForMenuOptimized();
            var gfdsf = _categoryService.GetCategoriesWithSubcategoriesForMenu();

            return HandleResponse(response);
        }
    }
}
