using eShop.Application.DTOs.Auth;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Auth;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerAuthService : IAuthService
{
    Task<ApiResponse<RegisterDTO>> RegisterAsync(UserRegisterRequest request);
}
