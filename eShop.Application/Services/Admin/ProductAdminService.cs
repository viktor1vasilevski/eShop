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
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObject;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Admin;

public class ProductAdminService(IEfUnitOfWork _uow, IEfRepository<Category> _categoryRepository, 
    IEfRepository<Product> _productRepository, IOpenAIProductDescriptionGenerator _openAIProductDescriptionGenerator) : IProductAdminService
{

    public async Task<ApiResponse<List<ProductAdminResponse>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Product>(request.SortBy, request.SortDirection);

        var (products, totalCount) = await _productRepository.QueryAsync(
            queryBuilder: q => q
                .Where(x => !x.IsDeleted)
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.Value.ToLower().Contains(request.Name.ToLower())),
            selector: x => new ProductAdminResponse
            {
                Id = x.Id,
                Name = x.Name.Value,
                Description = x.Description.Value,
                UnitPrice = x.UnitPrice.Value,
                UnitQuantity = x.UnitQuantity.Value,
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

        return new ApiResponse<List<ProductAdminResponse>>
        {
            Data = products.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<ProductDetailsAdminResponse>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            includeBuilder: q => q.Include(p => p.Category),
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductDetailsAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        var (categories, _) = await _categoryRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => c,
            cancellationToken: cancellationToken
        );

        var lookup = categories.ToDictionary(c => c.Id, c => c);

        var pathItems = Category.BuildPath(product.CategoryId, lookup);

        var productDto = new ProductDetailsAdminResponse
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description.Value,
            UnitPrice = product.UnitPrice.Value,
            UnitQuantity = product.UnitQuantity.Value,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            Categories = pathItems.Select(p => new CategoryRefDto { Id = p.Id, Name = p.Name }).ToList(),
            Created = product.Created,
            LastModified = product.LastModified
        };

        return new ApiResponse<ProductDetailsAdminResponse>
        {
            Status = ResponseStatus.Success,
            Data = productDto
        };
    }

    public async Task<ApiResponse<ProductEditAdminResponse>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductEditAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        var dto = new ProductEditAdminResponse
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description.Value,
            UnitPrice = product.UnitPrice.Value,
            UnitQuantity = product.UnitQuantity.Value,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            CategoryId = product.CategoryId
        };

        return new ApiResponse<ProductEditAdminResponse>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    public async Task<ApiResponse<ProductAdminResponse>> CreateProductAsync(CreateProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLowerInvariant();

        var categoryExists = await _categoryRepository.ExistsAsync(
            c => c.Id == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (!categoryExists)
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var hasChildren = await _categoryRepository.ExistsAsync(
            c => c.ParentCategoryId == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (hasChildren)
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = AdminProductConstants.ProductsAllowedOnlyOnLeafCategories
            };

        var nameTaken = await _productRepository.ExistsAsync(
            p => p.CategoryId == request.CategoryId &&
                 !p.IsDeleted &&
                 p.Name.Value.ToLower() == normalizedName,
            cancellationToken: cancellationToken
        );

        if (nameTaken)
            return new ApiResponse<ProductAdminResponse>
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

            await _productRepository.AddAsync(product, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.Created,
                Message = AdminProductConstants.ProductSuccessfullyCreated,
                Data = new ProductAdminResponse
                {
                    Id = product.Id,
                    Name = product.Name.Value,
                    Description = product.Description.Value,
                    UnitPrice = product.UnitPrice.Value,
                    UnitQuantity = product.UnitQuantity.Value,
                    Created = product.Created
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<ProductAdminResponse>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };

        product.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);

        return new ApiResponse<ProductAdminResponse>
        {
            Status = ResponseStatus.Success,
            Message = AdminProductConstants.ProductSuccessfullyDeleted
        };
    }

    public async Task<ApiResponse<ProductAdminResponse>> UpdateProductAsync(Guid id, UpdateProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => !p.IsDeleted && p.Id == id,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
        {
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminProductConstants.ProductDoesNotExist
            };
        }

        if (!string.Equals(product.Name.Value, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var nameTaken = await _productRepository.ExistsAsync(
                p => p.CategoryId == request.CategoryId &&
                     !p.IsDeleted &&
                     p.Name.Value.ToLower() == request.Name.Trim().ToLower() &&
                     p.Id != id,
                cancellationToken: cancellationToken
            );

            if (nameTaken)
            {
                return new ApiResponse<ProductAdminResponse>
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

            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.Success,
                Message = AdminProductConstants.ProductSuccessfullyUpdated
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<ProductAdminResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<string>> GenerateAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var response = await _openAIProductDescriptionGenerator.GenerateOpenAIProductDescriptionAsync(request, cancellationToken);

        if (response.StartsWith("OpenAI API error:", StringComparison.OrdinalIgnoreCase))
        {
            return new ApiResponse<string>
            {
                Status = ResponseStatus.Error,
                Message = response
            };
        }

        return new ApiResponse<string>
        {
            Status = ResponseStatus.Success,
            Data = response
        };
    }

}
