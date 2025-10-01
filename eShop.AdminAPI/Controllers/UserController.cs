using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UserController(IUserAdminService _userAdminService) : BaseController
{

    [HttpGet]
    public IActionResult Get([FromQuery] UserRequest request)
    {
        var response = _userAdminService.GetUsers(request);
        return HandleResponse(response);
    }

}
