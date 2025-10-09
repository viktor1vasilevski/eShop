using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

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
        var orderBy = SortHelper.BuildSort<Product>(request.SortBy, request.SortDirection);

        var (products, totalCount) = await _productAdminService.QueryAsync(
            queryBuilder: q => q
                .Where(x => !x.IsDeleted)
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
            selector: x => new ProductAdminDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                UnitPrice = x.UnitPrice,
                UnitQuantity = x.UnitQuantity,
                Image = ImageDataUriBuilder.FromImage(x.Image),
                Category = x.Category.Name,
                Created = x.Created,
                LastModified = x.LastModified
            },
            includeBuilder: x => x.Include(x => x.Category),
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take
        );

        return new ApiResponse<List<ProductAdminDto>>
        {
            Data = products.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }


    public async Task<ApiResponse<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        throw new NotImplementedException();
    }
}
