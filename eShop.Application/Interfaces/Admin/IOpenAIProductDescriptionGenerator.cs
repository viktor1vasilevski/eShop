using eShop.Application.Requests.AI;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces.Admin;

public interface IOpenAIProductDescriptionGenerator
{
    Task<ApiResponse<string>> GenerateDescriptionAsync(GenerateProductDescriptionRequest request);
}
