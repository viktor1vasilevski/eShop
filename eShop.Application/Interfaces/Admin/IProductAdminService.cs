using eShop.Application.DTOs.Product;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;

namespace eShop.Application.Interfaces.Admin;

public interface IProductAdminService
{
    ApiResponse<List<ProductAdminDto>> GetProducts(ProductAdminRequest request);
    Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductEditAdminDto>> GetProductForEditAsync(Guid id);



    // ova treba popravajne
    Task<ApiResponse<ProductAdminDto>> CreateProduct(CreateProductRequest request);
    Task<ApiResponse<ProductAdminDto>> UpdateProduct(Guid id, UpdateProductRequest request);
    ApiResponse<ProductAdminDto> DeleteProduct(Guid id);
}
