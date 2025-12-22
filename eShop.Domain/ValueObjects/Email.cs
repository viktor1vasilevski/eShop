using eShop.Domain.Exceptions;
using System.Text.RegularExpressions;

namespace eShop.Domain.ValueObjects;

public sealed class Email : Primitives.ValueObject
{
    public string Value { get; }
    private Email() { }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Email cannot be empty.");

        var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z]{2,})+$", RegexOptions.Compiled);
        if (!emailRegex.IsMatch(value))
            throw new DomainValidationException("Email format is invalid.");

        Value = value.Trim().ToLower();
    }

    public static Email Create(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
