using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using eShop.Application.Responses.Customer.Comment;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController(ICustomerCommentService _customerCommentService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CommentCustomerDto>>>> Get([FromQuery] CommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _customerCommentService.GetCommentsAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<CommentCustomerDto>>> Create([FromBody] CreateCommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
            return HandleResponse(new ApiResponse<CommentCustomerDto>
            {
                Status = ResponseStatus.Unauthorized,
                Message = CustomerAuthConstants.UserNotAuthenticated
            });

        var response = await _customerCommentService.CreateCommentAsync(userId.Value, request, cancellationToken);
        return HandleResponse(response);
    }
}
