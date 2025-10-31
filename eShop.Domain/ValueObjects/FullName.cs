using eShop.Domain.Exceptions;


namespace eShop.Domain.ValueObjects
{
    public sealed class FullName : Primitives.ValueObject
    {
        public string FirstName { get; }
        public string LastName { get; }

        private FullName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainValidationException("First name must be provided.");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainValidationException("Last name must be provided.");

            if (firstName.Length > 100)
                throw new DomainValidationException("First name cannot exceed 100 characters.");

            if (lastName.Length > 100)
                throw new DomainValidationException("Last name cannot exceed 100 characters.");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public static FullName Create(string firstName, string lastName) =>
            new(firstName, lastName);

        public string DisplayName => $"{FirstName} {LastName}";

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName.ToLowerInvariant();
            yield return LastName.ToLowerInvariant();
        }

        public override string ToString() => DisplayName;
    }
}
