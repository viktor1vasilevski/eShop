using eShop.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace eShop.Domain.ValueObjects;

public sealed class Email : Primitives.ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Email is required.");

        // Basic email format validation
        var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        if (!regex.IsMatch(value))
            throw new DomainValidationException($"Invalid email format: {value}");

        Value = value.ToLowerInvariant();
    }

    public static Email Create(string email) => new(email);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
