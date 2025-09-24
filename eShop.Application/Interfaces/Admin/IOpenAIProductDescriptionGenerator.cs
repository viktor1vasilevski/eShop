using eShop.Application.Requests.AI;

namespace eShop.Application.Interfaces.Admin;

public interface IOpenAIProductDescriptionGenerator
{
    Task<ApiResponse<string>> GenerateDescriptionAsync(GenerateProductDescriptionRequest request);
}
