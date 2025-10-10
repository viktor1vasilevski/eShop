using eShop.Application.Constants.Customer;
using eShop.Application.Constants.Shared;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Customer;

public class AuthCustomerService(IUnitOfWork _uow, IPasswordHasher _passwordHasher, IConfiguration _configuration) : IAuthCustomerService
{
    private readonly IEfRepository<User> _userRepository = _uow.GetEfRepository<User>();


    public async Task<ApiResponse<LoginDto>> LoginAsync(UserLoginRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null || user?.Username != request.Username || user?.Role != Role.Customer ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
        {
            return new ApiResponse<LoginDto>
            {
                Message = SharedConstants.InvalidCredentials,
                Status = ResponseStatus.Unauthorized
            };
        }

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDto>
        {
            Status = ResponseStatus.Success,
            Message = CustomerAuthConstants.CustomerLoggedSuccessfully,
            Data = new LoginDto
            {
                Id = user.Id,
                Token = token,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            }
        };
    }
}
