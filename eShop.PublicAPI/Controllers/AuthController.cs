using eShop.Application.Interfaces;
using eShop.Application.Requests.Auth;
using Microsoft.AspNetCore.Mvc;

namespace eShop.PublicAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(ICustomerAuthService _customerAuthService) : BaseController
{

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
    {
        var response = await _customerAuthService.LoginAsync(request);
        return HandleResponse(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] UserRegisterRequest request)
    {
        var response = await _customerAuthService.RegisterAsync(request);
        return HandleResponse(response);
    }
}
