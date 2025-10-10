using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IProductAdminService
{
    // Queries
    Task<ApiResponse<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductEditAdminDto>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> GenerateAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken = default);

    // Commands
    Task<ApiResponse<ProductAdminDto>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductAdminDto>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}

