using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController(IProductCustomerService _productCustomerService) : BaseController
{


    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductCustomerDto>>>> Get([FromQuery] ProductCustomerRequest request)
    {
        var response = await _productCustomerService.GetProductsAsync(request);
        return HandleResponse(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailsCustomerDto>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<ProductDetailsCustomerDto>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _productCustomerService.GetProductByIdAsync(id, userId.Value, cancellationToken);
        return HandleResponse(response);
    }
}
