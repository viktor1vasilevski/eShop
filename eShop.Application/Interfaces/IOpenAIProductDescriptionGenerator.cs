using eShop.Application.Requests.AI;

namespace eShop.Application.Interfaces;

public interface IOpenAIProductDescriptionGenerator
{
    Task<ApiResponse<string>> GenerateDescriptionAsync(GenerateProductDescriptionRequest request);
}
