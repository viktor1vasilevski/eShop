using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class CategoryController(IAdminCategoryService _adminCategoryService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] CategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.GetCategoriesAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.GetCategoryByIdAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateCategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.CreateCategoryAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCategoryAdminRequest request, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.UpdateCategoryAsync(id, request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("{id}/edit")]
    public async Task<ActionResult> GetCategoryForEdit(Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.GetCategoryForEditAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.DeleteCategoryAsync(id, cancellationToken);
        return HandleResponse(response);
    }

    [HttpGet("tree")]
    public async Task<ActionResult> GetCategoryTree(CancellationToken cancellationToken)
    {
        var response = await _adminCategoryService.GetCategoryTreeAsync(cancellationToken);
        return HandleResponse(response);
    }
}
