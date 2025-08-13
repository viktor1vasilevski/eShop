using eShop.Application.Constants;
using eShop.Application.DTOs.Auth;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Auth;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace eShop.Application.Services;

public class CustomerAuthService(IUnitOfWork _uow, IConfiguration _configuration, ILogger<AdminAuthService> _logger) : ICustomerAuthService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();

    public async Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var response = await _userRepository.GetAsync(x => x.Username == normalizedUsername);
        var user = response?.FirstOrDefault();

        if (user is null || user?.Role.ToString() != Role.Customer.ToString() || !user.VerifyPassword(request.Password))
        {
            return new ApiResponse<LoginDTO>
            {
                NotificationType = NotificationType.Unauthorized,
                Message = AuthConstants.INVALID_CREDENTIAL,
            };
        }

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDTO>
        {
            NotificationType = NotificationType.Success,
            Message = AuthConstants.CUSTOMER_LOGIN_SUCCESS,
            Data = new LoginDTO
            {
                Id = user.Id,
                Token = token,
                Email = user.Email,
                Username = user.Username,
                Role = user.Role
            }
        };
    }

    public async Task<ApiResponse<RegisterDTO>> RegisterAsync(UserRegisterRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usersExist = await _userRepository.ExistsAsync(x => x.Username == normalizedUsername || x.Email == normalizedEmail);

        if (usersExist)
            return new ApiResponse<RegisterDTO>
            {
                NotificationType = NotificationType.Conflict,
                Message = AuthConstants.ACCOUNT_ALREADY_EXISTS
            };

        try
        {
            var userData = new UserData(
                firstName: request.FirstName,
                lastName: request.LastName,
                username: request.Username,
                email: request.Email,
                password: request.Password,
                isActive: request.IsActive,
                role: Role.Customer);

            var user = User.CreateNew(userData);

            await _userRepository.InsertAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<RegisterDTO>
            {
                NotificationType = NotificationType.Success,
                Message = AuthConstants.CUSTOMER_REGISTER_SUCCESS,
                Data = new RegisterDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<RegisterDTO>
            {
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }
}
