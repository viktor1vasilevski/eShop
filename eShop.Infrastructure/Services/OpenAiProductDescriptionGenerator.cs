using eShop.Application.Interfaces;
using System.Text;
using System.Text.Json;

namespace eShop.Infrastructure.Services;

public class OpenAiProductDescriptionGenerator(HttpClient _httpClient, string _apiKey) : IProductDescriptionGenerator
{
    public async Task<string> GenerateDescriptionAsync(string productName, string category, string subcategory, string? additionalContext = null)
    {
        var prompt = BuildPrompt(productName, category, subcategory, additionalContext);

        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful assistant that generates product descriptions." },
                new { role = "user", content = prompt }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString()
                  ?? string.Empty;
    }

    private string BuildPrompt(string productName, string category, string subcategory, string? additionalContext)
    {
        var context = string.IsNullOrWhiteSpace(additionalContext) ? "" : $" Additional details: {additionalContext}.";
        return $"Generate a concise, attractive product description for a product.\n\n" +
               $"Product: {productName}\n" +
               $"Category: {category}\n" +
               $"Subcategory: {subcategory}\n" +
               $"{context}";
    }
}
