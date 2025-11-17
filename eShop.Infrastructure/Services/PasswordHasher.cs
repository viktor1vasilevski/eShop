using eShop.Application.Interfaces.Shared;
using eShop.Domain.Exceptions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace eShop.Infrastructure.Services;

public class PasswordHasher : IPasswordService
{
    private const string PasswordPattern =
    @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{4,}$";

    public void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainValidationException("Password cannot be empty.");

        if (!Regex.IsMatch(password, PasswordPattern))
            throw new DomainValidationException(
                "Password must contain at least one uppercase letter, one lowercase letter, " +
                "one number, one special character, and be at least 4 characters long."
            );
    }

    public string HashPassword(string password, out string salt)
    {
        byte[] saltBytes = GenerateSalt();
        string hash = Hash(password, saltBytes);
        salt = Convert.ToBase64String(saltBytes);
        return hash;
    }

    public bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        byte[] saltBytes = Convert.FromBase64String(storedSalt);
        string computedHash = Hash(password, saltBytes);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(storedHash),
            Convert.FromBase64String(computedHash));
    }

    private static string Hash(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(32));
    }

    private static byte[] GenerateSalt(int size = 16)
    {
        byte[] salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}
