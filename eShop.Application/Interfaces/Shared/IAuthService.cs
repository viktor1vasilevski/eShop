using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses;
using eShop.Application.Responses.Shared.Auth;

namespace eShop.Application.Interfaces.Shared;

public interface IAuthService
{
    Task<ApiResponse<LoginDto>> LoginAsync(UserLoginRequest request);
}
