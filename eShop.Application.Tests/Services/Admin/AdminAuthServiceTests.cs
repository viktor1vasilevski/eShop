using eShop.Application.Constants.Shared;
using eShop.Application.Enums;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Services.Admin;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Linq.Expressions;

namespace eShop.Application.Tests.Services.Admin;

public class AdminAuthServiceTests
{
    private readonly Mock<IEfUnitOfWork> _uowMock = new();
    private readonly Mock<IEfRepository<User>> _userRepoMock = new();
    private readonly Mock<eShop.Application.Interfaces.Shared.IPasswordService> _passwordServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly AdminAuthService _sut;

    public AdminAuthServiceTests()
    {
        _sut = new AdminAuthService(
            _uowMock.Object,
            _userRepoMock.Object,
            _passwordServiceMock.Object,
            _configMock.Object);
    }

    [Fact]
    public async Task LoginAsync_UserNotFound_ReturnsUnauthorized()
    {
        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = "admin", Password = "pass" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
        Assert.Equal(SharedConstants.InvalidCredentials, result.Message);
    }

    [Fact]
    public async Task LoginAsync_UserIsNotAdmin_ReturnsUnauthorized()
    {
        var customer = CreateTestUser(Role.Customer);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = customer.Username.Value, Password = "pass" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsUnauthorized()
    {
        var admin = CreateTestUser(Role.Admin);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _passwordServiceMock
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = admin.Username.Value, Password = "wrong" });

        Assert.Equal(ResultStatus.Unauthorized, result.Status);
    }

    [Fact]
    public async Task LoginAsync_ValidAdminCredentials_ReturnsSuccessWithToken()
    {
        var admin = CreateTestUser(Role.Admin);

        _userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _passwordServiceMock
            .Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _configMock
            .Setup(c => c["JwtSettings:Secret"])
            .Returns("super-secret-key-that-is-at-least-32-chars-long!");

        var result = await _sut.LoginAsync(new UserLoginRequest { Username = admin.Username.Value, Password = "Admin1@" });

        Assert.Equal(ResultStatus.Success, result.Status);
        Assert.NotNull(result.Data?.Token);
        Assert.Equal(admin.Username.Value, result.Data?.Username);
        Assert.Equal(Role.Admin, result.Data?.Role);
    }

    private static User CreateTestUser(Role role) =>
        User.CreateNew(new UserData("John", "Doe", "admin", "admin@test.com", "hash", "salt", role));
}
