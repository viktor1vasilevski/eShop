using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Admin.Category;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICustomerCategoryService _customerCategoryService) : BaseController
{

    [HttpGet("category-tree-for-menu")]
    public async Task<ActionResult> GetCategoryTreeForMenu(CancellationToken cancellationToken)
    {
        var response = await _customerCategoryService.GetCategoryTreeForMenuAsync(cancellationToken);
        return HandleResponse(response);
    }
}
