using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObjects
{
    public sealed class ProductName : Primitives.ValueObject
    {
        public string Value { get; }

        private ProductName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainValidationException("Product name cannot be empty.");

            if (value.Length > 50)
                throw new DomainValidationException("Product name cannot exceed 50 characters.");

            Value = value.Trim();
        }

        public static ProductName Create(string value) => new(value);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}
