using eShop.Domain.Enums;
using eShop.Domain.Exceptions;
using eShop.Domain.Helpers;
using eShop.Domain.Models.Base;
using eShop.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace eShop.Domain.Models;

public class User : AuditableBaseEntity
{
    public FullName FullName { get; private set; }
    public Username Username { get; private set; }
    public Role Role { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string SaltKey { get; private set; }
    public bool IsDeleted { get; private set; }


    public virtual Basket? Basket { get; private set; }


    private readonly List<Order> _orders = [];
    public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();


    private readonly List<Comment> _comments = [];
    public IReadOnlyCollection<Comment> Comments => _comments.AsReadOnly();


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
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(user.PasswordHash, nameof(user.PasswordHash));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(user.Salt, nameof(user.Salt));
        DomainValidatorHelper.ThrowIfEnumIsNotDefined(user.Role, nameof(user.Role));

        Username = Username.Create(user.Username);
        Email = Email.Create(user.Email);
        FullName = FullName.Create(user.FirstName, user.LastName);
        Role = user.Role;
        PasswordHash = user.PasswordHash;
        SaltKey = user.Salt;
    }

    public static void ValidatePassword(string password)
    {
        const string passwordPattern = @"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9])(?=.*?[#?!@$ %^&*-]).{4,}$";
        if (!Regex.IsMatch(password, passwordPattern))
            throw new DomainValidationException("Password must contain at least one uppercase, one lowercase, one number, one special character, and be at least 4 characters long.");
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
