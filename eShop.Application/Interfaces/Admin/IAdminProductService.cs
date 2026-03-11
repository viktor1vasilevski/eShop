using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IAdminProductService
{
    Task<Result<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ProductEditAdminDto>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<string>> GenerateAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductAdminDto>> CreateProductAsync(CreateProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductAdminDto>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}

