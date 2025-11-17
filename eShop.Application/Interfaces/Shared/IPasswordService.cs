namespace eShop.Application.Interfaces.Shared;

public interface IPasswordService
{
    string HashPassword(string password, out string salt);
    bool VerifyPassword(string password, string storedHash, string storedSalt);
    void ValidatePassword(string password);
}
