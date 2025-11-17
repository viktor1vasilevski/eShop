using eShop.Application.Constants.Customer;
using eShop.Application.Constants.Shared;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Customer.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Customer;

public class AuthCustomerService(IEfUnitOfWork _uow, IEfRepository<User> _userRepository, IPasswordService _passwordService, 
    IConfiguration _configuration) : IAuthCustomerService
{

    public async Task<ApiResponse<LoginResponse>> LoginAsync(UserLoginRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username.Value == request.Username);

        if (user is null || user?.Username.Value != request.Username || user?.Role != Role.Customer ||
            !_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
        {
            return new ApiResponse<LoginResponse>
            {
                Message = SharedConstants.InvalidCredentials,
                Status = ResponseStatus.Unauthorized
            };
        }

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginResponse>
        {
            Status = ResponseStatus.Success,
            Message = CustomerAuthConstants.CustomerLoggedSuccessfully,
            Data = new LoginResponse
            {
                Id = user.Id,
                Token = token,
                Email = user.Email.Value,
                Username = user.Username.Value,
                Role = user.Role
            }
        };
    }

    public async Task<ApiResponse<RegisterCustomerResponse>> RegisterCustomerAsync(CustomerRegisterRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usersExist = await _userRepository.ExistsAsync(x => x.Username.Value == normalizedUsername || x.Email.Value == normalizedEmail);

        if (usersExist)
            return new ApiResponse<RegisterCustomerResponse>
            {
                Status = ResponseStatus.Conflict,
                Message = CustomerAuthConstants.AccountAlreadyExists
            };

        try
        {
            _passwordService.ValidatePassword(request.Password);
            var passwordHash = _passwordService.HashPassword(request.Password, out string salt);

            var userData = new UserData(
                firstName: request.FirstName,
                lastName: request.LastName,
                username: normalizedUsername,
                email: normalizedEmail,
                passwordHash: passwordHash,
                salt: salt,
                role: Role.Customer);

            var user = User.CreateNew(userData);

            await _userRepository.AddAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<RegisterCustomerResponse>
            {
                Status = ResponseStatus.Success,
                Message = CustomerAuthConstants.CustomerRegisterSuccess,
                Data = new RegisterCustomerResponse
                {
                    Id = user.Id,
                    FirstName = user.FullName.FirstName,
                    LastName = user.FullName.LastName,
                    Username = user.Username.Value,
                    Email = user.Email.Value
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<RegisterCustomerResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

}
