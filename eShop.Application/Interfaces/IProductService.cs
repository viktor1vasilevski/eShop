using eShop.Application.DTOs.Product;
using eShop.Application.Requests.Product;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IProductService
{
    ApiResponse<List<ProductDetailsDTO>> GetProducts(ProductRequest request);
    ApiResponse<ProductDetailsDTO> GetProductById(Guid id);
    ApiResponse<ProductDetailsDTO> CreateProduct(CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> UpdateProduct(Guid id, CreateUpdateProductRequest request);
    ApiResponse<ProductDetailsDTO> DeleteProduct(Guid id);
}
