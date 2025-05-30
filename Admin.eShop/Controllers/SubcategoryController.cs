﻿using Admin.eShop.Controllers;
using eShop.Main.Interfaces;
using eShop.Main.Requests.Subcategory;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Admin.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class SubcategoryController(ISubcategoryService subcategoryService) : BaseController
    {
        private readonly ISubcategoryService _subcategoryService = subcategoryService;


        [HttpGet("Get")]
        public IActionResult Get([FromQuery] SubcategoryRequest request)
        {
            var response = _subcategoryService.GetSubcategories(request);
            return HandleResponse(response);
        }

        [HttpPost("Create")]
        public IActionResult Create([FromBody] CreateSubcategoryRequest request)
        {
            var response = _subcategoryService.CreateSubcategory(request);
            return HandleResponse(response);
        }

        [HttpGet("Get/{id}")]
        public IActionResult GetById([FromRoute] Guid id)
        {
            var response = _subcategoryService.GetSubcategoryById(id);
            return HandleResponse(response);
        }
    }
}
