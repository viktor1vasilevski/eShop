using eShop.Application.DTOs.Auth;
using eShop.Application.Requests.Auth;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerAuthService : IAuthService
{
    Task<ApiResponse<RegisterDTO>> RegisterAsync(UserRegisterRequest request);
}
