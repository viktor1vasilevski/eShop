using eShop.Application.DTOs.Auth;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Auth;
using eShop.Application.Responses;

namespace eShop.Application.Services;

public class AdminAuthService : IAuthService
{
    public async Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request)
    {
        throw new NotImplementedException();
    }
}
