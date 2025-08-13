using eShop.Application.Interfaces;
using eShop.Application.Requests.Auth;
using Microsoft.AspNetCore.Mvc;

namespace eShop.AdminAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthService _authService) : BaseController
{


    [HttpPost("admin/login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return HandleResponse(response);
    }
}
