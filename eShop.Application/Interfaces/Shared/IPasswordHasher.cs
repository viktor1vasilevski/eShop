namespace eShop.Application.Interfaces.Shared;

public interface IPasswordHasher
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string storedHash, string storedSalt);
}
