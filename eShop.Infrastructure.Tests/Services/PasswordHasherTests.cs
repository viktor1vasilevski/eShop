using eShop.Domain.Exceptions;
using eShop.Infrastructure.Services;

namespace eShop.Infrastructure.Tests.Services;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    // ── ValidatePassword ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("Admin@1")]
    [InlineData("Pass1!xyz")]
    [InlineData("A1!aaaa")]
    public void ValidatePassword_ValidPassword_DoesNotThrow(string password)
    {
        var ex = Record.Exception(() => _sut.ValidatePassword(password));
        Assert.Null(ex);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidatePassword_EmptyOrWhitespace_ThrowsDomainValidationException(string password)
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword(password));
    }

    [Fact]
    public void ValidatePassword_Null_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword(null!));
    }

    [Fact]
    public void ValidatePassword_MissingUppercase_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword("admin@1"));
    }

    [Fact]
    public void ValidatePassword_MissingLowercase_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword("ADMIN@1"));
    }

    [Fact]
    public void ValidatePassword_MissingDigit_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword("Admin@abc"));
    }

    [Fact]
    public void ValidatePassword_MissingSpecialCharacter_ThrowsDomainValidationException()
    {
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword("Admin123"));
    }

    [Fact]
    public void ValidatePassword_TooShort_ThrowsDomainValidationException()
    {
        // 3 chars — below the minimum of 4
        Assert.Throws<DomainValidationException>(() => _sut.ValidatePassword("A1!"));
    }

    // ── HashPassword ─────────────────────────────────────────────────────────

    [Fact]
    public void HashPassword_ValidPassword_ReturnsNonEmptyHashAndSalt()
    {
        var hash = _sut.HashPassword("Admin@1", out string salt);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.False(string.IsNullOrWhiteSpace(salt));
    }

    [Fact]
    public void HashPassword_SamePasswordCalledTwice_ProducesDifferentSalts()
    {
        _sut.HashPassword("Admin@1", out string salt1);
        _sut.HashPassword("Admin@1", out string salt2);

        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void HashPassword_SamePasswordCalledTwice_ProducesDifferentHashes()
    {
        var hash1 = _sut.HashPassword("Admin@1", out _);
        var hash2 = _sut.HashPassword("Admin@1", out _);

        Assert.NotEqual(hash1, hash2);
    }

    // ── VerifyPassword ───────────────────────────────────────────────────────

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.HashPassword("Admin@1", out string salt);

        Assert.True(_sut.VerifyPassword("Admin@1", hash, salt));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Admin@1", out string salt);

        Assert.False(_sut.VerifyPassword("Wrong@1", hash, salt));
    }

    [Fact]
    public void VerifyPassword_TamperedHash_ReturnsFalse()
    {
        _sut.HashPassword("Admin@1", out string salt);
        var tamperedHash = Convert.ToBase64String(new byte[32]); // all zeros

        Assert.False(_sut.VerifyPassword("Admin@1", tamperedHash, salt));
    }

    [Fact]
    public void VerifyPassword_WrongSalt_ReturnsFalse()
    {
        var hash = _sut.HashPassword("Admin@1", out _);
        _sut.HashPassword("Other@1", out string otherSalt);

        Assert.False(_sut.VerifyPassword("Admin@1", hash, otherSalt));
    }
}
