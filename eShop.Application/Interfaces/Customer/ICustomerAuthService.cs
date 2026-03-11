using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Responses.Customer.Auth;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerAuthService : IAuthService
{
    Task<Result<RegisterCustomerDto>> RegisterCustomerAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default);
}
