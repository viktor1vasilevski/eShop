using System.Security.Cryptography;

namespace eShop.Domain.Helpers;

public static class PasswordHelper
{
    public static string HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(hash);
    }

    public static bool VerifyPassword(string inputPassword, string storedHash, string storedSalt)
    {
        byte[] salt = Convert.FromBase64String(storedSalt);
        byte[] storedHashBytes = Convert.FromBase64String(storedHash);

        string computedHash = HashPassword(inputPassword, salt);
        byte[] computedHashBytes = Convert.FromBase64String(computedHash);

        return CryptographicOperations.FixedTimeEquals(storedHashBytes, computedHashBytes);
    }

    public static byte[] GenerateSalt(int size = 16)
    {
        byte[] salt = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }
}
