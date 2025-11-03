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
public class CommentController(ICommentCustomerService _commentCustomerService) : BaseController
{

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CommentCustomerResponse>>>> Get([FromQuery] CommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _commentCustomerService.GetCommentsAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    public async Task<ActionResult<ApiResponse<CommentCustomerResponse>>> Create([FromBody] CreateCommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _commentCustomerService.CreateCommentAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
