using eShop.Application.Constants.Admin;
using eShop.Application.DTOs.Admin.Category;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Product;
using eShop.Application.Responses.Admin.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Admin;

public class AdminProductService(IEfUnitOfWork _uow, IEfRepository<Category> _categoryRepository,
    IEfRepository<Product> _productRepository, IOpenAIProductDescriptionGenerator _openAIProductDescriptionGenerator) : IAdminProductService
{
    public async Task<Result<List<ProductAdminDto>>> GetProductsAsync(ProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Product>(request.SortBy, request.SortDirection);

        var (products, totalCount) = await _productRepository.QueryAsync(
            queryBuilder: q => q
                .Where(x => !x.IsDeleted)
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.Value.ToLower().Contains(request.Name.ToLower())),
            selector: x => new ProductAdminDto
            {
                Id = x.Id,
                Name = x.Name.Value,
                Description = x.Description.Value,
                UnitPrice = x.UnitPrice.Value,
                UnitQuantity = x.UnitQuantity.Value,
                Image = ImageDataUriBuilder.FromImage(x.Image),
                Category = x.Category.Name.Value,
                Created = x.Created,
                LastModified = x.LastModified
            },
            includeBuilder: x => x.Include(x => x.Category),
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return Result<List<ProductAdminDto>>.Success(products, totalCount);
    }

    public async Task<Result<ProductDetailsAdminDto>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            includeBuilder: q => q.Include(p => p.Category)
                                  .Include(p => p.Comments).ThenInclude(c => c.User),
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return Result<ProductDetailsAdminDto>.NotFound(AdminProductConstants.ProductDoesNotExist);

        var (categories, _) = await _categoryRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => c,
            cancellationToken: cancellationToken
        );

        var lookup = categories.ToDictionary(c => c.Id, c => c);
        var pathItems = Category.BuildPath(product.CategoryId, lookup);

        var productDto = new ProductDetailsAdminDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description.Value,
            UnitPrice = product.UnitPrice.Value,
            UnitQuantity = product.UnitQuantity.Value,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            Categories = pathItems.Select(p => new CategoryRefDto { Id = p.Id, Name = p.Name }).ToList(),
            Created = product.Created,
            LastModified = product.LastModified,
            Comments = product.Comments
                .OrderByDescending(c => c.Created)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    Username = c.User.Username.Value,
                    CommentText = c.Text.Value,
                    Rating = c.Rating,
                    Created = c.Created
                })
                .ToList()
        };

        return Result<ProductDetailsAdminDto>.Success(productDto);
    }

    public async Task<Result<ProductEditAdminDto>> GetProductForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return Result<ProductEditAdminDto>.NotFound(AdminProductConstants.ProductDoesNotExist);

        return Result<ProductEditAdminDto>.Success(new ProductEditAdminDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description.Value,
            UnitPrice = product.UnitPrice.Value,
            UnitQuantity = product.UnitQuantity.Value,
            Image = ImageDataUriBuilder.FromImage(product.Image),
            CategoryId = product.CategoryId
        });
    }

    public async Task<Result<ProductAdminDto>> CreateProductAsync(CreateProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLowerInvariant();

        var categoryExists = await _categoryRepository.ExistsAsync(
            c => c.Id == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (!categoryExists)
            return Result<ProductAdminDto>.NotFound(AdminCategoryConstants.CategoryDoesNotExist);

        var hasChildren = await _categoryRepository.ExistsAsync(
            c => c.ParentCategoryId == request.CategoryId && !c.IsDeleted,
            cancellationToken: cancellationToken
        );

        if (hasChildren)
            return Result<ProductAdminDto>.BadRequest(AdminProductConstants.ProductsAllowedOnlyOnLeafCategories);

        var nameTaken = await _productRepository.ExistsAsync(
            p => p.CategoryId == request.CategoryId &&
                 !p.IsDeleted &&
                 p.Name.Value.ToLower() == normalizedName,
            cancellationToken: cancellationToken
        );

        if (nameTaken)
            return Result<ProductAdminDto>.Conflict(AdminProductConstants.ProductExist);

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

        return Result<ProductAdminDto>.Created(new ProductAdminDto
        {
            Id = product.Id,
            Name = product.Name.Value,
            Description = product.Description.Value,
            UnitPrice = product.UnitPrice.Value,
            UnitQuantity = product.UnitQuantity.Value,
            Created = product.Created
        }, message: AdminProductConstants.ProductSuccessfullyCreated);
    }

    public async Task<Result<ProductAdminDto>> DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => p.Id == id && !p.IsDeleted,
            selector: p => p,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return Result<ProductAdminDto>.NotFound(AdminProductConstants.ProductDoesNotExist);

        product.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<ProductAdminDto>.Success(null!, message: AdminProductConstants.ProductSuccessfullyDeleted);
    }

    public async Task<Result<ProductAdminDto>> UpdateProductAsync(Guid id, UpdateProductAdminRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetSingleAsync(
            filter: p => !p.IsDeleted && p.Id == id,
            selector: p => p,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        if (product is null)
            return Result<ProductAdminDto>.NotFound(AdminProductConstants.ProductDoesNotExist);

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
                return Result<ProductAdminDto>.Conflict(AdminProductConstants.ProductExist);
        }

        Image? image = null;
        if (!string.IsNullOrEmpty(request.Image))
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            image = Image.FromBytes(bytes, type);
        }

        product.Update(request.Name.Trim(), request.Description?.Trim() ?? string.Empty, request.Price, request.Quantity, request.CategoryId, image);

        _productRepository.Update(product);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result<ProductAdminDto>.Success(null!, message: AdminProductConstants.ProductSuccessfullyUpdated);
    }

    public async Task<Result<string>> GenerateAIProductDescriptionAsync(GenerateAIProductDescriptionRequest request, CancellationToken cancellationToken)
    {
        var description = await _openAIProductDescriptionGenerator.GenerateOpenAIProductDescriptionAsync(request, cancellationToken);
        return Result<string>.Success(description);
    }
}
