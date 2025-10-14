using eShop.Application.Constants.Admin;
using eShop.Application.Constants.Shared;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Admin;

public class AuthAdminService(IUnitOfWork _uow, IEfRepository<User> _userRepository, 
    IPasswordHasher _passwordHasher, IConfiguration _configuration) : IAuthAdminService
{

    public async Task<ApiResponse<LoginDto>> LoginAsync(UserLoginRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username == request.Username);

        if (user is null || user?.Username != request.Username || user?.Role != Role.Admin || 
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
            Message = AdminAuthConstants.AdminLoggedSuccessfully,
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