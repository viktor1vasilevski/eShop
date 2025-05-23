﻿using Admin.eShop.Controllers;
using eShop.Main.Interfaces;
using eShop.Main.Requests.Category;
using Main.Enums;
using Main.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Admin.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[Route("api/[controller]")]
[ApiController]
public class CategoryController(ICategoryService categoryService) : BaseController
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet("Get")]
    public IActionResult Get([FromQuery] CategoryRequest request)
    {
        var response = _categoryService.GetCategories(request);
        return HandleResponse(response);
    }

    [HttpPost("Create")]
    public IActionResult Create([FromBody] CreateCategoryRequest request)
    {
        var response = _categoryService.CreateCategory(request);
        return HandleResponse(response);
    }

    [HttpPut("Edit/{id}")]
    public IActionResult Edit([FromRoute] Guid id, [FromBody] EditCategoryRequest request)
    {
        var response = _categoryService.EditCategory(id, request);
        return HandleResponse(response);
    }

    [HttpGet("Get/{id}")]
    public IActionResult GetById([FromRoute] Guid id)
    {
        var response = _categoryService.GetCategoryById(id);
        return HandleResponse(response);
    }

    [HttpDelete("Delete/{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        bool deleted = _categoryService.DeleteCategory(id);

        if (!deleted)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Entity not found",
                NotificationType = NotificationType.BadRequest
            });
        }

        return NoContent();
    }

    [HttpGet("GetCategoriesDropdownList")]
    public IActionResult GetCategoriesDropdownList()
    {
        var response = _categoryService.GetCategoriesDropdownList();
        return HandleResponse(response);
    }
}
