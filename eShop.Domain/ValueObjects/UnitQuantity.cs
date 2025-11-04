using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects;

public class UnitQuantity : Primitives.ValueObject
{
    public int Value { get; private set; }

    private UnitQuantity() { }

    private UnitQuantity(int value)
    {
        if (value <= 0)
            throw new DomainValidationException("Unit quantity must be greater than zero.");

        Value = value;
    }

    public static UnitQuantity Create(int value) => new UnitQuantity(value);

    public UnitQuantity Subtract(int amount)
    {
        if (amount <= 0)
            throw new DomainValidationException("Subtracted quantity must be greater than zero.");

        if (amount > Value)
            throw new DomainValidationException("Cannot subtract more than available quantity.");

        return new UnitQuantity(Value - amount);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();
}
