using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IProductAdminService
{
    Task<ApiResponse<List<ProductAdminResponse>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailsAdminResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductEditAdminResponse>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GenerateAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAdminResponse>> CreateProductAsync(CreateProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAdminResponse>> UpdateProductAsync(Guid id, UpdateProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAdminResponse>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}

