using eShop.Application.Interfaces.Shared;
using System.Security.Cryptography;

namespace eShop.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
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
