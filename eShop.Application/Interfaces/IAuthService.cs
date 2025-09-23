using eShop.Application.DTOs.Auth;
using eShop.Application.Requests.Auth;

namespace eShop.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request);
}
