using eShop.Application.DTOs.Auth;
using eShop.Application.Requests.Auth;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services;

public class CustomerAuthService(IUnitOfWork _uow, IConfiguration _configuration, ILogger<AdminAuthService> _logger,
    IPasswordHasher _passwordHasher) : ICustomerAuthService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public async Task<ApiResponse<LoginDTO>> LoginAsync(UserLoginRequest request)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var response = await _userRepository.GetAsync(x => x.Username == normalizedUsername);
        var user = response?.FirstOrDefault();

        if (user is null || user?.Role != Role.Customer || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
            return new ApiResponse<LoginDTO>
            {
                Status = ResponseStatus.Unauthorized,
                Message = AuthConstants.INVALID_CREDENTIAL,
            };

        var token = JwtTokenHelper.GenerateToken(_configuration, user);

        return new ApiResponse<LoginDTO>
        {
            Status = ResponseStatus.Success,
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
                Status = ResponseStatus.Conflict,
                Message = AuthConstants.ACCOUNT_ALREADY_EXISTS
            };

        try
        {
            var passwordHash = _passwordHasher.HashPassword(request.Password, out string salt);

            var userData = new UserData(
                firstName: request.FirstName,
                lastName: request.LastName,
                username: normalizedUsername,
                email: normalizedEmail,
                passwordHash: passwordHash,
                salt: salt,
                role: Role.Customer);

            var user = User.CreateNew(userData);

            await _userRepository.InsertAsync(user);
            await _uow.SaveChangesAsync();

            return new ApiResponse<RegisterDTO>
            {
                Status = ResponseStatus.Success,
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
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }
}
