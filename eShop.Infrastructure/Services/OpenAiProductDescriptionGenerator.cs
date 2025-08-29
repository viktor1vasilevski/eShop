using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace eShop.Infrastructure.Services;

public class OpenAiProductDescriptionGenerator : IProductDescriptionGenerator
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAiProductDescriptionGenerator(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"]
                  ?? throw new InvalidOperationException("OpenAI API key is missing in configuration.");
    }

    public async Task<ApiResponse<string>> GenerateDescriptionAsync(string productName, string category, string subcategory, string? additionalContext = null)
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
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                using var errorDoc = JsonDocument.Parse(responseContent);
                var errorMessage = errorDoc.RootElement
                    .GetProperty("error")
                    .GetProperty("message")
                    .GetString();

                return new ApiResponse<string>
                {
                    Message = $"OpenAI API error: {errorMessage}",
                    Status = ResponseStatus.ServerError
                };
            }
            catch
            {
                return new ApiResponse<string>
                {
                    Message = $"OpenAI API error: {response.StatusCode} : {responseContent}",
                    Status = ResponseStatus.ServerError
                };
            }
        }

        using var doc = JsonDocument.Parse(responseContent);

        var description = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(description))
        {
            return new ApiResponse<string>
            {
                Message = "No description generated.",
                Status = ResponseStatus.Info
            };
        }

        return new ApiResponse<string>
        {
            Status = ResponseStatus.Success,
            Data = description,
        };
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
