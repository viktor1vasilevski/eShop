using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Responses.Customer.Auth;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface IAuthCustomerService : IAuthService
{
    Task<ApiResponse<RegisterCustomerDto>> RegisterCustomerAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default);
}
