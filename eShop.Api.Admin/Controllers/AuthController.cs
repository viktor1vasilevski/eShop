using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eShop.Api.Admin.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class AuthController(IAuthAdminService _authAdminService) : BaseController
{


    [HttpPost("admin/login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] UserLoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authAdminService.LoginAsync(request, cancellationToken);
        return HandleResponse(response);
    }
}
