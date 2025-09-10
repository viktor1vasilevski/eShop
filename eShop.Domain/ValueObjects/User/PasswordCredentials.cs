using eShop.Domain.Helpers;

namespace eShop.Domain.ValueObjects.User;

public record PasswordCredentials
{
    public string Hash { get; }
    public string Salt { get; }


    private PasswordCredentials(string hash, string salt)
    {
        Hash = hash;
        Salt = salt;
    }

    public static PasswordCredentials Create(string hash, string salt)
    {
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(hash, nameof(hash));
        DomainValidatorHelper.ThrowIfNullOrWhiteSpace(salt, nameof(salt));

        return new PasswordCredentials(hash, salt);
    }
}
