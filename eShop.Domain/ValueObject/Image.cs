using eShop.Domain.Exceptions;

namespace eShop.Domain.ValueObject;

public sealed class Image
{
    public byte[] Bytes { get; }
    public string Type { get; }

    private Image() { }
    private Image(byte[] bytes, string type)
    {
        if (bytes == null || bytes.Length == 0)
            throw new DomainValidationException("Image must be provided.");

        var allowed = new[] { "jpeg", "png", "webp" };

        if (!allowed.Contains(type.ToLowerInvariant()))
            throw new DomainValidationException($"Unsupported image type: {type}");

        const int max = 5 * 1024 * 1024;

        if (bytes.Length > max)
            throw new DomainValidationException("Image size cannot exceed 5MB.");

        Bytes = bytes;
        Type = type.ToLowerInvariant();
    }

    public static Image FromBytes(byte[] bytes, string type) => new Image(bytes, type);
}
