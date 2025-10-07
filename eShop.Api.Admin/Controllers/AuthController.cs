using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IAuthService _authService) : BaseController
{


    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return HandleResponse(response);
    }
}
