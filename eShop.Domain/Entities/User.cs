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
    public Role Role { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string SaltKey { get; private set; } = string.Empty;
    public bool IsActive { get; set; } = false;

    public virtual Basket? Basket { get; set; }
    public virtual ICollection<Order>? Orders { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }

    private User() { }

    public static User CreateNew(UserData user)
    {
        var instance = new User();
        instance.Id = Guid.NewGuid();
        instance.ApplyUserData(user, isNewUser: true);
        return instance;
    }

    // Update existing user data, optionally allow password update
    public void Update(UserData user, bool isPasswordChangeAllowed = false)
    {
        ApplyUserData(user, isNewUser: false, isPasswordChangeAllowed);
    }

    private void ApplyUserData(UserData user, bool isNewUser, bool isPasswordChangeAllowed = false)
    {
        ValidateRequired(user.Username, nameof(user.Username));
        ValidateRequired(user.Email, nameof(user.Email));
        ValidateCoreFields(user.FirstName, user.LastName, user.Role);

        Username = user.Username.ToLower();
        Email = user.Email.ToLower();
        FirstName = user.FirstName;
        LastName = user.LastName;
        IsActive = user.IsActive;
        Role = user.Role;

        if (isNewUser || isPasswordChangeAllowed)
        {
            ValidateRequired(user.Password, "Password");
            var salt = PasswordHelper.GenerateSalt();
            PasswordHash = PasswordHelper.HashPassword(user.Password, salt);
            SaltKey = Convert.ToBase64String(salt);
        }
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


public class UserData
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Username { get; }
    public string Email { get; }
    public string Password { get; }
    public bool IsActive { get; set; }
    public Role Role { get; }

    public UserData(
        string firstName,
        string lastName,
        string username,
        string email,
        string password,
        bool isActive,
        Role role)
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Email = email;
        Password = password;
        IsActive = isActive;
        Role = role;
    }
}
