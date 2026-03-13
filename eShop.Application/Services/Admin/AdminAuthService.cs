using eShop.Application.Constants.Admin;
using eShop.Application.Constants.Shared;
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

public class AdminAuthService(IEfRepository<User> _userRepository,
    IPasswordService _passwordHasher, IConfiguration _configuration) : IAdminAuthService
{
    public async Task<Result<LoginDto>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username.Value == request.Username, cancellationToken);

        if (user is null || user?.Username.Value != request.Username || user?.Role != Role.Admin ||
            !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
        {
            return Result<LoginDto>.Unauthorized(SharedConstants.InvalidCredentials);
        }

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return Result<LoginDto>.Success(new LoginDto
        {
            Token = token,
            Email = user.Email.Value,
            Username = user.Username.Value,
            Role = user.Role
        }, message: AdminAuthConstants.AdminLoggedSuccessfully);
    }
}
