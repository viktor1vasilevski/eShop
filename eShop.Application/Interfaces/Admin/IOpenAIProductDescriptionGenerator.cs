using eShop.Application.Requests.Admin.Product;

namespace eShop.Application.Interfaces.Admin;

public interface IOpenAIProductDescriptionGenerator
{
    Task<string>  GenerateOpenAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken = default);
}
