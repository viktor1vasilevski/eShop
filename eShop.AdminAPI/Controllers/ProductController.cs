using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Requests.AI;
using eShop.Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class ProductController(IProductService _productService, IOpenAIProductDescriptionGenerator _openAIProductDescriptionGenerator) : BaseController
{


    [HttpGet]
    public IActionResult Get([FromQuery] ProductAdminRequest request)
    {
        var response = _productService.GetProducts(request);
        return HandleResponse(response);
    }

    //[HttpGet("{id}")]
    //public IActionResult GetById([FromRoute] Guid id, [FromQuery] Guid? userId = null)
    //{
    //    var response = _productService.GetProductById(id, userId);
    //    return HandleResponse(response);
    //}

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id)
    {
        var response = await _productService.GetProductByIdAsync(id);
        return HandleResponse(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateProductRequest request)
    {
        var response = await _productService.CreateProduct(request);
        if (response.Status == ResponseStatus.Created && response.Data?.Id != null)
        {
            var locationUri = Url.Action(nameof(GetByIdAsync), nameof(Product), new { id = response.Data.Id }, Request.Scheme);
            response.Location = locationUri;
        }
        return HandleResponse(response);
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] Guid id, [FromBody] CreateUpdateProductRequest request)
    {
        var response = _productService.UpdateProduct(id, request);
        return HandleResponse(response);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] Guid id)
    {
        var response = _productService.DeleteProduct(id);
        return HandleResponse(response);
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateDescription([FromQuery] GenerateProductDescriptionRequest request)
    {
        var response = await _openAIProductDescriptionGenerator.GenerateDescriptionAsync(request);
        return HandleResponse(response);
    }
}
