using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CommentController(ICommentCustomerService _commentCustomerService) : BaseController
{
    [HttpGet]
    public IActionResult Get([FromQuery] CommentRequest request)
    {
        var response = _commentCustomerService.GetComments(request);
        return HandleResponse(response);
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
    public IActionResult Create([FromBody] CreateCommentRequest request)
    {
        var response = _commentCustomerService.CreateComment(request);
        return HandleResponse(response);
    }
}
