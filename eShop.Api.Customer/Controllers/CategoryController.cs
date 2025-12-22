using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICustomerCategoryService _categoryCustomerService) : BaseController
{


    [HttpGet("category-tree-for-menu")]
    public async Task<ActionResult<ApiResponse<List<CategoryTreeDto>>>> GetCategoryTreeForMenu(CancellationToken cancellationToken)
    {
        var response = await _categoryCustomerService.GetCategoryTreeForMenuAsync(cancellationToken);
        return HandleResponse(response);
    }
}
