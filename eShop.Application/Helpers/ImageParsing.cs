namespace eShop.Application.Helpers;

public static class ImageParsing
{
    public static (byte[] bytes, string type) FromBase64(string? base64String)
    {
        if (string.IsNullOrWhiteSpace(base64String))
            return (Array.Empty<byte>(), string.Empty);

        string base64Data = base64String.Contains("base64,")
            ? base64String[(base64String.IndexOf("base64,") + 7)..]
            : base64String;

        var bytes = Convert.FromBase64String(base64Data);

        var parts = base64String.Split(';');
        var type = (parts.Length > 0 && parts[0].Contains("/"))
            ? parts[0].Split('/')[1]
            : string.Empty;

        return (bytes, type);
    }
}
