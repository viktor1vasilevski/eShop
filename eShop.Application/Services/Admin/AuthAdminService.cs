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
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Admin;

public class AuthAdminService(IEfUnitOfWork _uow, IEfRepository<User> _userRepository, 
    IPasswordService _passwordHasher, IConfiguration _configuration) : IAuthAdminService
{

    public async Task<ApiResponse<LoginResponse>> LoginAsync(UserLoginRequest request)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username.Value == request.Username);

        if (user is null || user?.Username.Value != request.Username || user?.Role != Role.Admin || 
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
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
            Message = AdminAuthConstants.AdminLoggedSuccessfully,
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
}