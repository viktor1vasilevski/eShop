using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Requests.Shared.Auth;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthCustomerService _customerAuthService) : BaseController
{


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        var response = await _customerAuthService.LoginAsync(request);
        return HandleResponse(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CustomerRegisterRequest request)
    {
        var response = await _customerAuthService.RegisterCustomerAsync(request);
        return HandleResponse(response);
    }
}
