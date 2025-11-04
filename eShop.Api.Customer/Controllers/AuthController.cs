using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Customer.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Customer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthCustomerService _customerAuthService) : BaseController
{


    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] UserLoginRequest request)
    {
        var response = await _customerAuthService.LoginAsync(request);
        return HandleResponsee(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterCustomerResponse>>> Register([FromBody] CustomerRegisterRequest request)
    {
        var response = await _customerAuthService.RegisterCustomerAsync(request);
        return HandleResponsee(response);
    }
}
