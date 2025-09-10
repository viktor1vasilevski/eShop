using eShop.Application.DTOs.Auth;
using eShop.Application.Requests.Auth;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services;

public class AdminAuthService(IUnitOfWork _uow, IConfiguration _configuration, IPasswordHasher _passwordHasher) : IAuthService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public async Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var response = await _userRepository.GetAsync(x => x.Username == normalizedUsername);
        var user = response?.FirstOrDefault();

        if (user is null || user?.Role != Role.Admin || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
            return new ApiResponse<LoginDTO>
            {
                Message = AuthConstants.INVALID_CREDENTIAL,
                Status = ResponseStatus.Unauthorized
            };

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDTO>
        {
            Status = ResponseStatus.Success,
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
