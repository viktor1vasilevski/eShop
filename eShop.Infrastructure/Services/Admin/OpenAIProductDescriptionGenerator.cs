﻿using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace eShop.Infrastructure.Services.Admin;

public class OpenAIProductDescriptionGenerator(HttpClient httpClient, IConfiguration configuration) : IOpenAIProductDescriptionGenerator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is missing in configuration.");


    public async Task<string> GenerateOpenAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You write natural, concise product descriptions for ecommerce." },
                new
                {
                    role = "user",
                    content = $"Write a short product description (max 2500 characters) for a product named \"{request.ProductName}\" in the category path \"{request.Categories}\". " +
                              "Do not include a title or heading—only the description text."
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Headers = { { "Authorization", $"Bearer {_apiKey}" } },
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                using var errorDoc = JsonDocument.Parse(responseContent);
                var errorMessage = errorDoc.RootElement
                    .GetProperty("error")
                    .GetProperty("message")
                    .GetString();

                return $"OpenAI API error: {errorMessage}";
            }
            catch
            {
                return $"OpenAI API error: {response.StatusCode} : {responseContent}";
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
            return "No description generated.";
        }

        return description.Trim();
    }
}
