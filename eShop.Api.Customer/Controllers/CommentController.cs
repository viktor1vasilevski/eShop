using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController(ICommentCustomerService _commentCustomerService) : BaseController
{

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] CommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _commentCustomerService.GetCommentsAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    public async Task<IActionResult> Create([FromBody] CreateCommentCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await _commentCustomerService.CreateCommentAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
