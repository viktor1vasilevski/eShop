using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Shared;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default);
}
