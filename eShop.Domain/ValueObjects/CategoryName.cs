using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public sealed class CategoryName : Primitives.ValueObject
{
    public string Value { get; }
    private CategoryName() { }
    private CategoryName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException("Category name cannot be empty.");

        if (value.Length < 2)
            throw new DomainValidationException("Category name must be at least 2 characters long.");

        if (value.Length > 200)
            throw new DomainValidationException("Category name cannot exceed 200 characters.");

        Value = value.Trim();
    }

    public static CategoryName Create(string value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
