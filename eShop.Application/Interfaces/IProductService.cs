using eShop.Application.DTOs.Product;
using eShop.Application.Requests.Product;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IProductService
{
    ApiResponse<List<ProductDto>> GetProducts(ProductRequest request);
    ApiResponse<ProductDetailsDTO> GetProductById(Guid id, Guid? userId);
    Task<ApiResponse<ProductDetailsDTO>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductDetailsDTO>> CreateProduct(CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> UpdateProduct(Guid id, CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> DeleteProduct(Guid id);
}
