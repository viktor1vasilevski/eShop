using eShop.Domain.Entities.Base;
using eShop.Domain.Enums;
using eShop.Domain.Exceptions;

namespace eShop.Domain.Entities;

public class User : AuditableBaseEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Username { get; private set; }
    public Role Role { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string SaltKey { get; private set; }
    public bool IsDeleted { get; set; }


    public virtual Basket? Basket { get; private set; }
    public virtual ICollection<Order>? Orders { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }

    private User() { }

    public static User CreateNew(UserData user)
    {
        var instance = new User();
        instance.Id = Guid.NewGuid();
        instance.ApplyUserData(user);
        return instance;
    }

    public void ClearBasket() => Basket?.ClearItems();


    private void ApplyUserData(UserData user)
    {
        ValidateRequired(user.FirstName, nameof(user.FirstName));
        ValidateRequired(user.LastName, nameof(user.LastName));
        ValidateRequired(user.Username, nameof(user.Username));
        ValidateRequired(user.Email, nameof(user.Email));
        ValidateRequired(user.PasswordHash, nameof(user.PasswordHash));
        ValidateRequired(user.Salt, nameof(user.Salt));

        if (!Enum.IsDefined(typeof(Role), user.Role))
            throw new DomainValidationException("Invalid user role specified.");

        Username = user.Username.ToLower();
        Email = user.Email.ToLower();
        FirstName = user.FirstName;
        LastName = user.LastName;
        Role = user.Role;
        PasswordHash = user.PasswordHash;
        SaltKey = user.Salt;
    }

    private static void ValidateRequired(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be empty.");
    }
}


public class UserData
{
    public string FirstName { get; }
    public string LastName { get; }
    public string Username { get; }
    public string Email { get; }
    public string PasswordHash { get; }
    public string Salt { get; }
    public Role Role { get; }

    public UserData(
        string firstName,
        string lastName,
        string username,
        string email,
        string passwordHash,
        string salt,
        Role role)
    {
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Salt = salt;
        Role = role;
    }
}
