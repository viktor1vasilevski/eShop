using eShop.Domain.Exceptions;

namespace eShop.Domain.Helpers;

public static class DomainValidatorHelper
{
    public static void ThrowIfNullOrWhiteSpace(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{fieldName} cannot be empty.");
    }

    public static void ThrowIfEmptyGuid(Guid guid, string fieldName)
    {
        if (guid == Guid.Empty)
            throw new DomainValidationException($"{fieldName} cannot be empty.");
    }
}
