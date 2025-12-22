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
public class AuthController(ICustomerAuthService _customerAuthService) : BaseController
{


    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginDto>>> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _customerAuthService.LoginAsync(request, cancellationToken);
        return HandleResponse(response);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterCustomerDto>>> Register([FromBody] CustomerRegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _customerAuthService.RegisterCustomerAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
