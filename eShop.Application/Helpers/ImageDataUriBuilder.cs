using eShop.Domain.ValueObject;

namespace eShop.Application.Helpers;

public static class ImageDataUriBuilder
{
    private static string? Build(byte[]? bytes, string? imageType)
    {
        if (bytes == null || bytes.Length == 0 || string.IsNullOrWhiteSpace(imageType))
            return null;

        var lower = imageType.Trim().ToLowerInvariant();
        string mime = lower.StartsWith("image/") ? lower : $"image/{lower}";
        var base64 = Convert.ToBase64String(bytes);
        return $"data:{mime};base64,{base64}";
    }

    public static string? FromImage(Image? image)
        => image is null ? null : Build(image.Bytes, image.Type);
}
