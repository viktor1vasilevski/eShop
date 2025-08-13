using eShop.Application.Constants;
using eShop.Application.DTOs.Auth;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Auth;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services;

public class AdminAuthService(IUnitOfWork _uow, IConfiguration _configuration) : IAuthService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public async Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var response = await _userRepository.GetAsync(x => x.Username == normalizedUsername);
        var user = response?.FirstOrDefault();

        if (user is null || user?.Role != Role.Admin || user.IsDeleted || !user.VerifyPassword(request.Password))
            return new ApiResponse<LoginDTO>
            {
                Message = AuthConstants.INVALID_CREDENTIAL,
                NotificationType = NotificationType.Unauthorized
            };

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDTO>
        {
            NotificationType = NotificationType.Success,
            Message = AuthConstants.ADMIN_LOGIN_SUCCESS,
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
}
