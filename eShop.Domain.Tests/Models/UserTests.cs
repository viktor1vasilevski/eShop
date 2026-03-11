using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Models;

namespace eShop.Domain.Tests.Models;

public class UserTests
{
    [Fact]
    public void CreateNew_ValidData_SetsPropertiesCorrectly()
    {
        var userData = new UserData("John", "Doe", "john", "john@test.com", "hash", "salt", Role.Customer);

        var user = User.CreateNew(userData);

        Assert.Equal("john", user.Username.Value);
        Assert.Equal("john@test.com", user.Email.Value);
        Assert.Equal("John", user.FullName.FirstName);
        Assert.Equal("Doe", user.FullName.LastName);
        Assert.Equal(Role.Customer, user.Role);
        Assert.Equal("hash", user.PasswordHash);
        Assert.Equal("salt", user.SaltKey);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void CreateNew_EmptyPasswordHash_ThrowsDomainValidationException()
    {
        var userData = new UserData("John", "Doe", "john", "john@test.com", "", "salt", Role.Customer);
        Assert.Throws<DomainValidationException>(() => User.CreateNew(userData));
    }

    [Fact]
    public void CreateNew_EmptySalt_ThrowsDomainValidationException()
    {
        var userData = new UserData("John", "Doe", "john", "john@test.com", "hash", "", Role.Customer);
        Assert.Throws<DomainValidationException>(() => User.CreateNew(userData));
    }

    [Fact]
    public void CreateNew_AdminRole_SetsRoleCorrectly()
    {
        var userData = new UserData("Jane", "Doe", "admin", "admin@test.com", "hash", "salt", Role.Admin);

        var user = User.CreateNew(userData);

        Assert.Equal(Role.Admin, user.Role);
    }
}
