using eShop.Application.Constants.Customer;
using eShop.Application.Constants.Shared;
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

public class CustomerAuthService(IEfUnitOfWork _uow, IEfRepository<User> _userRepository, IPasswordService _passwordService,
    IConfiguration _configuration) : ICustomerAuthService
{
    public async Task<Result<LoginDto>> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FirstOrDefaultAsync(x => x.Username.Value == request.Username, cancellationToken);

        if (user is null || user?.Username.Value != request.Username || user?.Role != Role.Customer ||
            !_passwordService.VerifyPassword(request.Password, user.PasswordHash, user.SaltKey))
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
        }, message: CustomerAuthConstants.CustomerLoggedSuccessfully);
    }

    public async Task<Result<RegisterCustomerDto>> RegisterCustomerAsync(CustomerRegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var usersExist = await _userRepository.ExistsAsync(
            x => x.Username.Value == normalizedUsername || x.Email.Value == normalizedEmail,
            cancellationToken);

        if (usersExist)
            return Result<RegisterCustomerDto>.Conflict(CustomerAuthConstants.AccountAlreadyExists);

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

            await _userRepository.AddAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<RegisterCustomerDto>.Success(new RegisterCustomerDto
            {
                Id = user.Id,
                FirstName = user.FullName.FirstName,
                LastName = user.FullName.LastName,
                Username = user.Username.Value,
                Email = user.Email.Value
            }, message: CustomerAuthConstants.CustomerRegisterSuccess);
        }
        catch (DomainValidationException ex)
        {
            return Result<RegisterCustomerDto>.BadRequest(ex.Message);
        }
    }
}
