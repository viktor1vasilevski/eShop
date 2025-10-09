using eShop.Application.Constants.Admin;
using eShop.Application.DTOs.Admin.Category;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using eShop.Domain.ValueObject;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Admin;

public class ProductAdminService(IUnitOfWork _uow) : IProductAdminService
{
    private readonly IEfRepository<Category> _categoryAdminService = _uow.GetEfRepository<Category>();
    private readonly IEfRepository<Product> _productAdminService = _uow.GetEfRepository<Product>();


    public async Task<ApiResponse<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default)
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
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return new ApiResponse<List<ProductAdminDto>>
        {
            Data = products.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productAdminService.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            includeBuilder: q => q.Include(p => p.Category),
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductDetailsAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        var (categories, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => c,
            cancellationToken: cancellationToken
        );

        var lookup = categories.ToDictionary(c => c.Id, c => c);

        var pathItems = Category.BuildPath(product.CategoryId, lookup);

        var productDto = new ProductDetailsAdminDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            UnitQuantity = product.UnitQuantity,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            Categories = pathItems.Select(p => new CategoryRefDto { Id = p.Id, Name = p.Name }).ToList(),
            Created = product.Created,
            LastModified = product.LastModified
        };

        return new ApiResponse<ProductDetailsAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = productDto
        };
    }

    public async Task<ApiResponse<ProductEditAdminDto>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productAdminService.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductEditAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        var dto = new ProductEditAdminDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            UnitPrice = product.UnitPrice,
            UnitQuantity = product.UnitQuantity,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            CategoryId = product.CategoryId
        };

        return new ApiResponse<ProductEditAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    public async Task<ApiResponse<ProductAdminDto>> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLowerInvariant();

        var categoryExists = await _categoryAdminService.ExistsAsync(
            c => c.Id == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (!categoryExists)
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var hasChildren = await _categoryAdminService.ExistsAsync(
            c => c.ParentCategoryId == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (hasChildren)
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = AdminProductConstants.ProductsAllowedOnlyOnLeafCategories
            };

        var nameTaken = await _productAdminService.ExistsAsync(
            p => p.CategoryId == request.CategoryId &&
                 !p.IsDeleted &&
                 p.Name.ToLower() == normalizedName,
            cancellationToken: cancellationToken
        );

        if (nameTaken)
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = AdminProductConstants.ProductExist
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var product = Product.Create(
                name: trimmedName,
                description: request.Description?.Trim() ?? string.Empty,
                unitPrice: request.Price,
                unitQuantity: request.Quantity,
                categoryId: request.CategoryId,
                image: image!
            );

            await _productAdminService.AddAsync(product, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.Created,
                Message = AdminProductConstants.ProductSuccessfullyCreated,
                Data = new ProductAdminDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    UnitPrice = product.UnitPrice,
                    UnitQuantity = product.UnitQuantity,
                    Created = product.Created
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<ProductAdminDto>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productAdminService.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        product.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ProductAdminDto>
        {
            Status = ResponseStatus.Success,
            Message = AdminProductConstants.ProductSuccessfullyDeleted
        };
    }

    public async Task<ApiResponse<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productAdminService.GetSingleAsync(
            filter: p => !p.IsDeleted && p.Id == id,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
        {
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };
        }

        if (!string.Equals(product.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameTaken = await _productAdminService.ExistsAsync(
                p => p.CategoryId == request.CategoryId &&
                     !p.IsDeleted &&
                     p.Name.ToLower() == request.Name.Trim().ToLower() &&
                     p.Id != id,
                cancellationToken: cancellationToken
            );

            if (nameTaken)
            {
                return new ApiResponse<ProductAdminDto>
                {
                    Status = ResponseStatus.Conflict,
                    Message = AdminProductConstants.ProductExist
                };
            }
        }

        try
        {
            Image? image = null;
            if (!string.IsNullOrEmpty(request.Image))
            {
                var (bytes, type) = ImageParsing.FromBase64(request.Image);
                image = Image.FromBytes(bytes, type);
            }

            product.Update(
                request.Name.Trim(),
                request.Description?.Trim() ?? string.Empty,
                request.Price,
                request.Quantity,
                request.CategoryId,
                image
            );

            await _uow.SaveChangesAsync(cancellationToken);

            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.Success,
                Message = AdminProductConstants.ProductSuccessfullyUpdated
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductAdminDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }
}
