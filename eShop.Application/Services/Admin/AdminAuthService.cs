using eShop.Application.Constants;
using eShop.Application.Enums;
using eShop.Application.Helpers.Shared;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses;
using eShop.Application.Responses.Shared.Auth;
using eShop.Domain.Entities;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Admin;

public class AdminAuthService(IUnitOfWork _uow, IConfiguration _configuration, IPasswordHasher _passwordHasher) : IAuthService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public async Task<ApiResponse<LoginDto>> LoginAsync(UserLoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var response = await _userRepository.GetAsync(x => x.Username == normalizedUsername);
        var user = response?.FirstOrDefault();

        if (user is null || user?.Role != Role.Admin || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
            return new ApiResponse<LoginDto>
            {
                Message = AuthConstants.INVALID_CREDENTIAL,
                Status = ResponseStatus.Unauthorized
            };

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDto>
        {
            Status = ResponseStatus.Success,
            Message = AuthConstants.ADMIN_LOGIN_SUCCESS,
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
