using eShop.Application.Constants.Admin;
using eShop.Application.DTOs.Admin.Category;
using eShop.Application.DTOs.Admin.Product;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using eShop.Domain.ValueObject;
using Microsoft.EntityFrameworkCore;
using static eShop.Domain.Models.Category;

namespace eShop.Application.Services.Admin;

public class CategoryAdminService(IEfUnitOfWork _uow, IEfRepository<Category> _categoryRepository, IEfRepository<Product> _productRepository) : ICategoryAdminService
{

    public async Task<ApiResponse<List<CategoryAdminResponse>>> GetCategoriesAsync(CategoryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Category>(request.SortBy, request.SortDirection);

        var (categories, totalCount) = await _categoryRepository.QueryAsync(
            queryBuilder: q => q
                .Where(x => !x.IsDeleted)
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.Value.ToLower().Contains(request.Name.ToLower())),
            selector: c => new CategoryAdminResponse
            {
                Id = c.Id,
                Name = c.Name.Value,
                Created = c.Created,
                LastModified = c.LastModified
            },
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return new ApiResponse<List<CategoryAdminResponse>>
        {
            Data = categories.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CategoryAdminResponse>> CreateCategoryAsync(CreateCategoryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLower();

        if (await _categoryRepository.ExistsAsync(
                x => x.Name.Value.ToLower() == normalizedName &&
                     x.ParentCategoryId == request.ParentCategoryId &&
                     !x.IsDeleted,
                cancellationToken))
        {
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.Conflict,
                Message = AdminCategoryConstants.CategoryExist
            };
        }

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Category.Create(trimmedName, image, request.ParentCategoryId);

            await _categoryRepository.AddAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.Created,
                Message = AdminCategoryConstants.CategorySuccessfullyCreated,
                Data = new CategoryAdminResponse
                {
                    Id = category.Id,
                    Name = category.Name.Value,
                    Created = category.Created,
                    ParentCategoryId = category.ParentCategoryId
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<CategoryAdminResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var normalizedName = trimmedName.ToLower();

        if (await _categoryRepository.ExistsAsync(x =>
                x.Id != id &&
                x.ParentCategoryId == request.ParentCategoryId &&
                x.Name.Value.ToLower() == normalizedName &&
                !x.IsDeleted,
                cancellationToken))
        {
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.Conflict,
                Message = AdminCategoryConstants.CategoryExist
            };
        }

        try
        {
            Image? image = null;
            if (!string.IsNullOrEmpty(request.Image))
            {
                var (bytes, type) = ImageParsing.FromBase64(request.Image);
                image = Image.FromBytes(bytes, type);
            }

            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == id)
                {
                    return new ApiResponse<CategoryAdminResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = AdminCategoryConstants.CategoryCannotBeOwnParent
                    };
                }

                var allCategories = await _categoryRepository.QueryAsync(
                    queryBuilder: q => q.Where(c => !c.IsDeleted),
                    selector: c => new Category.CategoryNode(c.Id, c.ParentCategoryId),
                    cancellationToken: cancellationToken
                );

                var descendants = Category.GetDescendantIds(allCategories.Items, id);
                if (descendants.Contains(request.ParentCategoryId.Value))
                {
                    return new ApiResponse<CategoryAdminResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = AdminCategoryConstants.CategoryCannotBeMovedUnderDescendant
                    };
                }
            }

            category.Update(trimmedName, image, request.ParentCategoryId);
            _categoryRepository.Update(category);
            await _uow.SaveChangesAsync(cancellationToken);

            // 6️⃣ Return result
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.Success,
                Message = AdminCategoryConstants.CategorySuccessfullyUpdated,
                Data = new CategoryAdminResponse
                {
                    Id = category.Id,
                    Name = category.Name.Value,
                    Created = category.Created,
                    LastModified = category.LastModified,
                    ParentCategoryId = category.ParentCategoryId
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<CategoryDetailsAdminResponse>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetSingleAsync(
            filter: c => c.Id == id && !c.IsDeleted,
            includeBuilder: q => q.Include(c => c.Products)
                                  .Include(c => c.Children)
                                  .AsNoTracking(),
            selector: c => new CategoryDetailsAdminResponse
            {
                Id = c.Id,
                Name = c.Name.Value,
                Image = ImageDataUriBuilder.FromImage(c.Image),
                ParentCategoryId = c.ParentCategoryId,
                Created = c.Created,
                LastModified = c.LastModified,
                Products = c.Products
                    .OrderBy(p => p.Name.Value)
                    .Select(p => new ProductRefDto
                    {
                        Id = p.Id,
                        Name = p.Name.Value
                    })
                    .ToList(),
                Children = c.Children
                    .OrderBy(c => c.Name.Value)
                    .Select(c => new CategoryRefDto
                    {
                        Id = c.Id,
                        Name = c.Name.Value
                    })
                    .ToList()
            },
            cancellationToken: cancellationToken
        );

        if (category is null)
            return new ApiResponse<CategoryDetailsAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        return new ApiResponse<CategoryDetailsAdminResponse>
        {
            Status = ResponseStatus.Success,
            Data = category
        };
    }

    public async Task<ApiResponse<CategoryEditAdminResponse>> GetCategoryForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (allCategories, _) = await _categoryRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name.Value,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            },
            cancellationToken: cancellationToken
        );

        var entity = (await _categoryRepository.FindAsync(
            predicate: c => c.Id == id && !c.IsDeleted,
            cancellationToken: cancellationToken
        )).FirstOrDefault();

        if (entity == null)
            return new ApiResponse<CategoryEditAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var nodes = allCategories
            .Select(c => new CategoryNode(c.Id, c.ParentCategoryId))
            .ToList();

        var excludedIds = GetDescendantIds(nodes, id);

        var validCategories = allCategories
            .Where(c => !excludedIds.Contains(c.Id) && c.ProductCount == 0)
            .ToList();

        var validTree = BuildCategoryTree(validCategories);

        var dto = new CategoryEditAdminResponse
        {
            Id = entity.Id,
            Name = entity.Name.Value,
            ParentCategoryId = entity.ParentCategoryId,
            Image = ImageDataUriBuilder.FromImage(entity.Image),
            ValidParentTree = validTree
        };

        return new ApiResponse<CategoryEditAdminResponse>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    private List<CategoryTreeResponse> BuildCategoryTree(List<CategoryFlatDto> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .Select(c =>
            {
                var children = BuildCategoryTree(categories, c.Id);
                return new CategoryTreeResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    SubcategoryCount = children.Count,
                    ProductCount = c.ProductCount,
                    Children = children
                };
            })
            .ToList();
    }

    public async Task<ApiResponse<List<CategoryTreeResponse>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var (allCategories, _) = await _categoryRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name.Value,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            },
            cancellationToken: cancellationToken
        );

        var tree = BuildCategoryTree(allCategories.ToList());

        return new ApiResponse<List<CategoryTreeResponse>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CategoryAdminResponse>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryRepository.ExistsAsync(c => !c.IsDeleted && c.Id == id, cancellationToken);
        if (!exists)
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var (allCategories, _) = await _categoryRepository.QueryAsync(
            queryBuilder: c => c.Where(c => !c.IsDeleted),
            selector: c => new CategoryNode(c.Id, c.ParentCategoryId),
            cancellationToken: cancellationToken
        );

        var idsToDelete = Category.GetDescendantIds(allCategories.ToList(), id);

        var (productIds, _) = await _productRepository.QueryAsync(
            queryBuilder: p => p.Where(p => idsToDelete.Contains(p.CategoryId)),
            selector: p => p.Id,
            cancellationToken: cancellationToken
        );

        if (productIds.Any())
            return new ApiResponse<CategoryAdminResponse>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(AdminCategoryConstants.CategoryHasProducts, productIds.Count())
            };

        var (categoriesToDelete, _) = await _categoryRepository.QueryAsync(
            queryBuilder: c => c.Where(c => idsToDelete.Contains(c.Id)),
            selector: c => c,
            asNoTracking: false,
            cancellationToken: cancellationToken
        );

        Category.SoftDeleteRange(categoriesToDelete.ToList());
        await _uow.SaveChangesAsync(cancellationToken);

        var pluralSuffix = categoriesToDelete.Count() == 1 ? "y" : "ies";

        return new ApiResponse<CategoryAdminResponse>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(AdminCategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count(), pluralSuffix),
        };
    }
}
