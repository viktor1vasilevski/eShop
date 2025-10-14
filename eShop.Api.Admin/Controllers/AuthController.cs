using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IAuthAdminService _authAdminService) : BaseController
{


    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
    {
        var response = await _authAdminService.LoginAsync(request);
        return HandleResponse(response);
    }
}
