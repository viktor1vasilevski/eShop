using eShop.Domain.Entities.Base;
using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;

namespace eShop.Domain.Entities;

public class User : AuditableBaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public Role Role { get; set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string SaltKey { get; private set; } = string.Empty;

    private User() { }

    public static User CreateNew(string firstName, string lastName, string username, string email, string password, Role role)
    {
        ValidateRequired(username, nameof(username));
        ValidateRequired(email, nameof(email));
        ValidateRequired(password, "Password");
        ValidateCoreFields(firstName, lastName, role);

        var salt = PasswordHelper.GenerateSalt();
        var hash = PasswordHelper.HashPassword(password, salt);

        return new User
        {
            FirstName = firstName,
            LastName = lastName,
            Username = username.ToLower(),
            Email = email.ToLower(),
            PasswordHash = hash,
            SaltKey = Convert.ToBase64String(salt),
            Role = role
        };
    }

    public void ApplyChanges(string firstName, string lastName, Role role)
    {
        ValidateCoreFields(firstName, lastName, role);

        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }

    public bool VerifyPassword(string inputPassword)
    {
        return PasswordHelper.VerifyPassword(inputPassword, PasswordHash, SaltKey);
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be empty.");
    }

    private static void ValidateCoreFields(string firstName, string lastName, Role role)
    {
        ValidateRequired(firstName, nameof(firstName));
        ValidateRequired(lastName, nameof(lastName));

        if (!Enum.IsDefined(typeof(Role), role))
            throw new DomainValidationException("Invalid user role specified.");
    }
}
