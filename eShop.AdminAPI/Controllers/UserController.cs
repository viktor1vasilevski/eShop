using eShop.Application.Interfaces;
using eShop.Application.Requests.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UserController(IUserService _userService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] UserRequest request)
    {
        var response = _userService.GetUsers(request);
        return HandleResponse(response);
    }

}
