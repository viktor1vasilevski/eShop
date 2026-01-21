using eShop.Application.Exceptions;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace eShop.Infrastructure.Services.Admin;

public class OpenAIProductDescriptionGenerator(HttpClient httpClient, IConfiguration configuration) : IOpenAIProductDescriptionGenerator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is missing in configuration.");


    public async Task<string> GenerateOpenAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new 
                    { 
                        role = "system", 
                        content = "You write natural, concise product descriptions for ecommerce." 
                    },
                    new
                    {
                        role = "user",
                        content = $"Write a short product description (max 2500 characters) for a product named \"{request.ProductName}\" in the category path \"{request.Categories}\". " +
                                  "Do not include a title or heading—only the description text."
                    }
                }
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            requestMessage.Content = JsonContent.Create(requestBody);

            var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new ExternalDependencyException(
                    $"OpenAI returned error: {response.StatusCode}");
            }

            using var doc = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));

            var description = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ExternalDependencyException(
                    "OpenAI did not generate any description.");
            }

            return description.Trim();
        }
        catch (Exception ex)
        {
            throw new ExternalDependencyException("OpenAI API unreachable.", ex);
        }       
    }
}
