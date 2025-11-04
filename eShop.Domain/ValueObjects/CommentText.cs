using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public sealed class CommentText : Primitives.ValueObject
{
    public string Value { get; }

    private CommentText() { }

    private CommentText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Comment text cannot be empty.");

        if (value.Length > 2500)
            throw new DomainValidationException("Comment text cannot exceed 2500 characters.");

        Value = value.Trim();
    }

    public static CommentText Create(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
