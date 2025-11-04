using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public sealed class Username : Primitives.ValueObject
{
    public string Value { get; }
    private Username() { }

    private Username(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Username cannot be empty.");

        if (value.Length > 50)
            throw new DomainValidationException("Username cannot exceed 50 characters.");

        Value = value.Trim().ToLower();
    }

    public static Username Create(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
