using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public sealed class ProductDescription : Primitives.ValueObject
{
    public string Value { get; }

    private ProductDescription(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Product description cannot be empty.");

        if (value.Length > 2500)
            throw new DomainValidationException("Description cannot exceed 2500 characters.");

        Value = value;
    }

    public static ProductDescription Create(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
