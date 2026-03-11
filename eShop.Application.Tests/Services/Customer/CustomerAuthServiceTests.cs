using eShop.Application.Constants.Customer;
using eShop.Application.Constants.Shared;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Customer.Auth;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Services.Customer;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Customer;

public class CustomerAuthServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<User>> _userRepoMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly CustomerAuthService _sut;

    public CustomerAuthServiceTests()
    {
        _sut = new CustomerAuthService(
            _uowMock.Object,
            _userRepoMock.Object,
            _passwordServiceMock.Object,
            _configMock.Object);
    }

    // --- LoginAsync ---

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsUnauthorized()
    {
        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = "john", Password = "pass" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
        Assert.Equal(SharedConstants.InvalidCredentials, result.Message);
    }

    [Fact]
    public async Task LoginAsync_UserIsAdmin_ReturnsUnauthorized()
    {
        var admin = CreateTestUser(Role.Admin);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = admin.Username.Value, Password = "pass" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsUnauthorized()
    {
        var customer = CreateTestUser(Role.Customer);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _passwordServiceMock
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = customer.Username.Value, Password = "wrong" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ValidCustomerCredentials_ReturnsSuccessWithToken()
    {
        var customer = CreateTestUser(Role.Customer);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _passwordServiceMock
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _configMock
            .Setup(c => c["JwtSettings:Secret"])
            .Returns("super-secret-key-that-is-at-least-32-chars-long!");

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = customer.Username.Value, Password = "Customer1@" });

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data?.Token);
        Assert.Equal(Role.Customer, result.Data?.Role);
    }

    // --- RegisterCustomerAsync ---

    [Fact]
    public async Task RegisterCustomerAsync_UsernameOrEmailAlreadyExists_ReturnsConflict()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CustomerRegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john@test.com",
            Password = "Password1@"
        };

        var result = await _sut.RegisterCustomerAsync(request);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(CustomerAuthConstants.AccountAlreadyExists, result.Message);
    }

    [Fact]
    public async Task RegisterCustomerAsync_ValidRequest_ReturnsSuccess()
    {
        _userRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordServiceMock
            .Setup(p => p.ValidatePassword(It.IsAny<string>()));

        _passwordServiceMock
            .Setup(p => p.HashPassword(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns((string _, ref string salt) => { salt = "testsalt"; return "testhash"; });

        _uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var request = new CustomerRegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "john",
            Email = "john@test.com",
            Password = "Password1@"
        };

        var result = await _sut.RegisterCustomerAsync(request);

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.Equal(CustomerAuthConstants.CustomerRegisterSuccess, result.Message);
        Assert.Equal("john", result.Data?.Username);
    }

    private static User CreateTestUser(Role role) =>
        User.CreateNew(new UserData("John", "Doe", "john", "john@test.com", "hash", "salt", role));
}
