using eShop.Application.DTOs.Auth;
using eShop.Application.Requests.Auth;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request);
}
