using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface IProductAdminService
{
    // Queries
    Task<ApiResponse<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request);
    Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductEditAdminDto>> GetProductForEditAsync(Guid id);


    // Commands
    Task<ApiResponse<ProductAdminDto>> CreateProductAsync(CreateProductRequest request);
    Task<ApiResponse<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task<ApiResponse<ProductAdminDto>> DeleteProductAsync(Guid id);
}
