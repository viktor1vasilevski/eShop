using eShop.Application.Interfaces.Customer;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryCustomerService _categoryCustomerService) : BaseController
{


    [HttpGet("categoryTreeForMenu")]
    public async Task<IActionResult> GetCategoryTreeForMenu(CancellationToken cancellationToken)
    {
        var response = await _categoryCustomerService.GetCategoryTreeForMenuAsync(cancellationToken);
        return HandleResponse(response);
    }
}
