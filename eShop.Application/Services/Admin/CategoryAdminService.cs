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
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using eShop.Domain.ValueObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static eShop.Domain.Models.Category;

namespace eShop.Application.Services.Admin;

public class CategoryAdminService(IUnitOfWork _uow, ILogger<CategoryAdminService> _logger) : ICategoryAdminService
{
    private readonly IEfRepository<Category> _categoryAdminService = _uow.GetEfRepository<Category>();
    private readonly IEfRepository<Product> _productAdminService = _uow.GetEfRepository<Product>();


    public async Task<ApiResponse<List<CategoryAdminDto>>> GetCategoriesAsync(CategoryAdminRequest request)
    {
        var orderBy = SortHelper.BuildSort<Category>(request.SortBy, request.SortDirection);

        var (categories, totalCount) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q
                .WhereIf(true, x => !x.IsDeleted)
                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
            selector: c => new CategoryAdminDto
            {
                Id = c.Id,
                Name = c.Name,
                Created = c.Created,
                LastModified = c.LastModified
            },
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take
        );

        return new ApiResponse<List<CategoryAdminDto>>
        {
            Data = categories.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CategoryAdminDto>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLower();

        if (await _categoryAdminService.ExistsAsync(x => x.Name.ToLower() == normalizedName &&
                x.ParentCategoryId == request.ParentCategoryId && !x.IsDeleted))
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = AdminCategoryConstants.CategoryExist,
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Category.Create(trimmedName, image, request.ParentCategoryId);
            await _categoryAdminService.AddAsync(category);
            await _uow.SaveChangesAsync();

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Created,
                Message = AdminCategoryConstants.CategorySuccessfullyCreated,
                Data = new CategoryAdminDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Created = category.Created,
                    ParentCategoryId = category.ParentCategoryId,
                }
            };

        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating category");
            throw;
        }
    }

    public async Task<ApiResponse<CategoryAdminDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _categoryAdminService.GetByIdAsync(id);
        if (category is null)
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var normalizedName = trimmedName.ToLower();

        if (await _categoryAdminService.ExistsAsync(x =>
            x.Id != id &&
            x.ParentCategoryId == request.ParentCategoryId &&
            x.Name.ToLower() == normalizedName &&
            !x.IsDeleted))
        {
            return new ApiResponse<CategoryAdminDto>
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

            if (request.ParentCategoryId.HasValue && request.ParentCategoryId == id)
            {
                return new ApiResponse<CategoryAdminDto>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = AdminCategoryConstants.CategoryCannotBeOwnParent
                };
            }

            if (request.ParentCategoryId.HasValue)
            {
                var all = await _categoryAdminService.QueryAsync(queryBuilder: q => q.Where(c => !c.IsDeleted),
                        selector: c => new CategoryNode(c.Id, c.ParentCategoryId));

                var descendants = Category.GetDescendantIds(all.Items, id);
                if (descendants.Contains(request.ParentCategoryId.Value))
                {
                    return new ApiResponse<CategoryAdminDto>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = AdminCategoryConstants.CategoryCannotBeMovedUnderDescendant
                    };
                }
            }

            category.Update(trimmedName, image, request.ParentCategoryId);
            _categoryAdminService.Update(category);
            await _uow.SaveChangesAsync();

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Success,
                Message = AdminCategoryConstants.CategorySuccessfullyUpdated,
                Data = new CategoryAdminDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Created = category.Created,
                    LastModified = category.LastModified,
                    ParentCategoryId = category.ParentCategoryId
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id)
    {
        var (categories, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q
                .Where(c => c.Id == id && !c.IsDeleted).AsNoTracking(),
            includeBuilder: c => c.Include(c => c.Products).Include(c => c.Children),
            selector: c => new CategoryDetailsAdminDto
            {
                Id = c.Id,
                Name = c.Name,
                Image = ImageDataUriBuilder.FromImage(c.Image),
                ParentCategoryId = c.ParentCategoryId,
                Created = c.Created,
                LastModified = c.LastModified,
                Products = c.Products
                    .OrderBy(p => p.Name)
                    .Select(p => new ProductRefDto { Id = p.Id, Name = p.Name })
                    .ToList(),
                Children = c.Children
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryRefDto { Id = c.Id, Name = c.Name })
                    .ToList()
            },
            take: 1
        );

        var category = categories.FirstOrDefault();

        if (category is null)
        {
            return new ApiResponse<CategoryDetailsAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };
        }

        return new ApiResponse<CategoryDetailsAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = category
        };
    }

    public async Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id)
    {
        // 1️⃣ Fetch all categories as flat DTOs
        var (allCategories, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            }
        );

        var entities = await _categoryAdminService.FindAsync(c => c.Id == id && !c.IsDeleted);
        var entity = entities.FirstOrDefault();

        if (entity == null)
            return new ApiResponse<CategoryEditAdminDto>
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

        var dto = new CategoryEditAdminDto
        {
            Id = entity.Id,
            Name = entity.Name,
            ParentCategoryId = entity.ParentCategoryId,
            Image = ImageDataUriBuilder.FromImage(entity.Image),
            ValidParentTree = validTree
        };

        return new ApiResponse<CategoryEditAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    private List<CategoryTreeDto> BuildCategoryTree(List<CategoryFlatDto> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .Select(c =>
            {
                var children = BuildCategoryTree(categories, c.Id);
                return new CategoryTreeDto
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

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync()
    {
        var (allCategories, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            }
        );

        var tree = BuildCategoryTree(allCategories.ToList());

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }


    public async Task<ApiResponse<CategoryAdminDto>> DeleteCategoryAsync(Guid id)
    {
        var exists = await _categoryAdminService.ExistsAsync(c => !c.IsDeleted && c.Id == id);
        if (!exists)
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };
        }

        var (allCategories, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: c => c.Where(c => !c.IsDeleted),
            selector: c => new CategoryNode(c.Id, c.ParentCategoryId)
        );

        var idsToDelete = Category.GetDescendantIds(allCategories.ToList(), id);

        var (productCount, _) = await _productAdminService.QueryAsync(
            queryBuilder: p => p.Where(p => idsToDelete.Contains(p.CategoryId)),
            selector: p => p.Id
        );

        if (productCount.Any())
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(AdminCategoryConstants.CategoryHasProducts, productCount.Count())
            };
        }

        var (categoriesToDelete, _) = await _categoryAdminService.QueryAsync(
            queryBuilder: c => c.Where(c => idsToDelete.Contains(c.Id)),
            selector: c => c);

        SoftDeleteRange(categoriesToDelete.ToList());

        await _uow.SaveChangesAsync();

        var pluralSuffix = categoriesToDelete.Count() == 1 ? "y" : "ies";

        return new ApiResponse<CategoryAdminDto>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(AdminCategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count(), pluralSuffix),
        };
    }
}
