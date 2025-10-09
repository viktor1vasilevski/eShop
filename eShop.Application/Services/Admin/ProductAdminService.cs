using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using Microsoft.Extensions.Logging;

namespace eShop.Application.Services.Admin;

public class ProductAdminService(IUnitOfWork _uow, ILogger<ProductAdminService> _logger) : IProductAdminService
{
    private readonly IEfRepository<Category> _categoryAdminService = _uow.GetEfRepository<Category>();
    private readonly IEfRepository<Product> _productAdminService = _uow.GetEfRepository<Product>();


    public async Task<ApiResponse<ProductAdminDto>> CreateProductAsync(CreateProductRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<ProductAdminDto>> DeleteProductAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<ProductEditAdminDto>> GetProductForEditAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        throw new NotImplementedException();
    }
}
