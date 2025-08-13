using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Subcategory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class SubcategoryController(ISubcategoryService _subcategoryService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] SubcategoryRequest request)
    {
        var response = _subcategoryService.GetSubcategories(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var response = _subcategoryService.GetSubcategoryById(id);
        return HandleResponse(response);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateUpdateSubcategoryRequest request)
    {
        var response = _subcategoryService.CreateSubcategory(request);
        if (response.NotificationType == NotificationType.Created && response.Data?.Id != null)
        {
            var locationUri = Url.Action(nameof(GetById), "Subcategory", new { id = response.Data.Id }, Request.Scheme);
            response.Location = locationUri;
        }
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] CreateUpdateSubcategoryRequest request)
    {
        var response = _subcategoryService.UpdateSubcategory(id, request);
        return HandleResponse(response);
    }


    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var response = _subcategoryService.DeleteSubcategory(id);
        return HandleResponse(response);
    }


    [HttpGet("DropdownList")]
    public IActionResult GetSubcategoriesDropdownList()
    {
        var response = _subcategoryService.GetSubcategoriesDropdownList();
        return HandleResponse(response);

    }

    [HttpGet("DropdownListWithCategories")]
    public IActionResult GetSubcategoriesWithCategoriesDropdownList()
    {
        var response = _subcategoryService.GetSubcategoriesWithCategoriesDropdownList();
        return HandleResponse(response);

    }
}
