using eShop.Application.DTOs.Product;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;

namespace eShop.Application.Interfaces.Admin;

public interface IProductAdminService
{
    Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id);



    // ova treba popravajne
    ApiResponse<List<ProductDto>> GetProducts(ProductAdminRequest request);
    Task<ApiResponse<ProductDetailsDTO>> CreateProduct(CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> UpdateProduct(Guid id, CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> DeleteProduct(Guid id);
}
