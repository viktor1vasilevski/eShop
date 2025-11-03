using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public class UnitPrice : Primitives.ValueObject
{
    public decimal Value { get; }

    private UnitPrice(decimal value)
    {
        if (value <= 0)
            throw new DomainValidationException("Unit price must be greater than zero.");

        Value = value;
    }

    public static UnitPrice Create(decimal value) => new(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("0.00");
}
