using eShop.Application.Enums;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.AI;
using eShop.Application.Responses;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace eShop.Infrastructure.Services;

public class OpenAIProductDescriptionGenerator(HttpClient httpClient, IConfiguration configuration) : IOpenAIProductDescriptionGenerator
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key is missing in configuration.");


    public async Task<ApiResponse<string>> GenerateDescriptionAsync(GenerateProductDescriptionRequest request)
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

        var response = await _httpClient.SendAsync(httpRequest);
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
            Data = description.Trim()
        };
    }


}
