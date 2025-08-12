using eShop.Application.Interfaces;
using eShop.Application.Requests.Comment;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController(ICommentService _commentService) : BaseController
    {


        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Customer")]
        public IActionResult Create([FromBody] CreateCommentRequest request)
        {
            var response = _commentService.CreateComment(request);
            return HandleResponse(response);
        }
    }
}
