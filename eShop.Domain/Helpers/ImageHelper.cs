using eShop.Domain.Exceptions;

namespace eShop.Domain.Helpers;

public static class ImageHelper
{
    /// <summary>
    /// Converts a base64 string to a byte array.
    /// Handles strings with or without the "base64," prefix.
    /// </summary>
    public static byte[] ConvertBase64ToBytes(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return Array.Empty<byte>();

        string base64Data = base64String.Contains("base64,")
            ? base64String.Substring(base64String.IndexOf("base64,") + 7)
            : base64String;

        return Convert.FromBase64String(base64Data);
    }

    /// <summary>
    /// Extracts the image type (e.g. png, jpeg) from a base64 string.
    /// </summary>
    public static string ExtractImageType(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return string.Empty;

        var parts = base64String.Split(';');
        if (parts.Length == 0 || !parts[0].Contains("/"))
            return string.Empty;

        var typeParts = parts[0].Split('/');
        return typeParts.Length > 1 ? typeParts[1] : string.Empty;
    }



    /// <summary>
    /// Validates image bytes and type against rules (size & allowed formats).
    /// Throws DomainValidationException if invalid.
    /// </summary>
    public static void ValidateImage(byte[] imageBytes, string imageType)
    {
        if (imageBytes == null || imageBytes.Length == 0)
            throw new DomainValidationException("Image must be provided.");

        const int maxImageSizeInBytes = 5 * 1024 * 1024; // 5MB
        if (imageBytes.Length > maxImageSizeInBytes)
            throw new DomainValidationException("Image size cannot exceed 5MB.");

        if (string.IsNullOrWhiteSpace(imageType))
            throw new DomainValidationException("Image type must be provided.");

        var allowedTypes = new[] { "jpeg", "png", "webp" };
        if (!allowedTypes.Contains(imageType.ToLower()))
            throw new DomainValidationException($"Unsupported image type: {imageType}");
    }
}
