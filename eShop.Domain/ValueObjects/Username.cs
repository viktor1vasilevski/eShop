using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public sealed class Username : Primitives.ValueObject
{
    public string Value { get; }

    private Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Username is required");
        if (value.Length > 100)
            throw new DomainValidationException("Username cannot exceed 100 characters");

        Value = value.ToLower();
    }

    public static Username Create(string username) => new(username);

    protected override IEnumerable<object> GetEqualityComponents() => new[] { Value };
}
