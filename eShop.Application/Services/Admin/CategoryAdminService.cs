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
using static eShop.Domain.Models.Category;

namespace eShop.Application.Services.Admin;

public class CategoryAdminService(IUnitOfWork _uow) : ICategoryAdminService
{
    private readonly IEfRepository<Category> _categoryAdminRepository = _uow.GetEfRepository<Category>();
    private readonly IEfRepository<Product> _productAdminRepository = _uow.GetEfRepository<Product>();


    public async Task<ApiResponse<List<CategoryAdminDto>>> GetCategoriesAsync(CategoryAdminRequest request, CancellationToken cancellationToken = default)
    {
        var orderBy = SortHelper.BuildSort<Category>(request.SortBy, request.SortDirection);

        var (categories, totalCount) = await _categoryAdminRepository.QueryAsync(
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
            take: request.Take,
            cancellationToken: cancellationToken
        );

        return new ApiResponse<List<CategoryAdminDto>>
        {
            Data = categories.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CategoryAdminDto>> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name.Trim();
        var normalizedName = trimmedName.ToLower();

        if (await _categoryAdminRepository.ExistsAsync(
                x => x.Name.ToLower() == normalizedName &&
                     x.ParentCategoryId == request.ParentCategoryId &&
                     !x.IsDeleted,
                cancellationToken))
        {
            return new ApiResponse<CategoryAdminDto>
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

            await _categoryAdminRepository.AddAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Created,
                Message = AdminCategoryConstants.CategorySuccessfullyCreated,
                Data = new CategoryAdminDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Created = category.Created,
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

    public async Task<ApiResponse<CategoryAdminDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _categoryAdminRepository.GetByIdAsync(id, cancellationToken);
        if (category is null)
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var normalizedName = trimmedName.ToLower();

        if (await _categoryAdminRepository.ExistsAsync(x =>
                x.Id != id &&
                x.ParentCategoryId == request.ParentCategoryId &&
                x.Name.ToLower() == normalizedName &&
                !x.IsDeleted,
                cancellationToken))
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

            if (request.ParentCategoryId.HasValue)
            {
                if (request.ParentCategoryId.Value == id)
                {
                    return new ApiResponse<CategoryAdminDto>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = AdminCategoryConstants.CategoryCannotBeOwnParent
                    };
                }

                var allCategories = await _categoryAdminRepository.QueryAsync(
                    queryBuilder: q => q.Where(c => !c.IsDeleted),
                    selector: c => new Category.CategoryNode(c.Id, c.ParentCategoryId),
                    cancellationToken: cancellationToken
                );

                var descendants = Category.GetDescendantIds(allCategories.Items, id);
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
            _categoryAdminRepository.Update(category);
            await _uow.SaveChangesAsync(cancellationToken);

            // 6️⃣ Return result
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

    public async Task<ApiResponse<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryAdminRepository.GetSingleAsync(
            filter: c => c.Id == id && !c.IsDeleted,
            includeBuilder: q => q.Include(c => c.Products)
                                  .Include(c => c.Children)
                                  .AsNoTracking(),
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
                    .Select(p => new ProductRefDto
                    {
                        Id = p.Id,
                        Name = p.Name
                    })
                    .ToList(),
                Children = c.Children
                    .OrderBy(c => c.Name)
                    .Select(c => new CategoryRefDto
                    {
                        Id = c.Id,
                        Name = c.Name
                    })
                    .ToList()
            },
            cancellationToken: cancellationToken
        );

        if (category is null)
            return new ApiResponse<CategoryDetailsAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        return new ApiResponse<CategoryDetailsAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = category
        };
    }

    public async Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var (allCategories, _) = await _categoryAdminRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            },
            cancellationToken: cancellationToken
        );

        var entity = (await _categoryAdminRepository.FindAsync(
            predicate: c => c.Id == id && !c.IsDeleted,
            cancellationToken: cancellationToken
        )).FirstOrDefault();

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

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var (allCategories, _) = await _categoryAdminRepository.QueryAsync(
            queryBuilder: q => q.Where(c => !c.IsDeleted),
            selector: c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            },
            cancellationToken: cancellationToken
        );

        var tree = BuildCategoryTree(allCategories.ToList());

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<CategoryAdminDto>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _categoryAdminRepository.ExistsAsync(c => !c.IsDeleted && c.Id == id, cancellationToken);
        if (!exists)
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = AdminCategoryConstants.CategoryDoesNotExist
            };

        var (allCategories, _) = await _categoryAdminRepository.QueryAsync(
            queryBuilder: c => c.Where(c => !c.IsDeleted),
            selector: c => new CategoryNode(c.Id, c.ParentCategoryId),
            cancellationToken: cancellationToken
        );

        var idsToDelete = Category.GetDescendantIds(allCategories.ToList(), id);

        var (productIds, _) = await _productAdminRepository.QueryAsync(
            queryBuilder: p => p.Where(p => idsToDelete.Contains(p.CategoryId)),
            selector: p => p.Id,
            cancellationToken: cancellationToken
        );

        if (productIds.Any())
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(AdminCategoryConstants.CategoryHasProducts, productIds.Count())
            };

        var (categoriesToDelete, _) = await _categoryAdminRepository.QueryAsync(
            queryBuilder: c => c.Where(c => idsToDelete.Contains(c.Id)),
            selector: c => c,
            cancellationToken: cancellationToken
        );

        Category.SoftDeleteRange(categoriesToDelete.ToList());
        await _uow.SaveChangesAsync(cancellationToken);

        var pluralSuffix = categoriesToDelete.Count() == 1 ? "y" : "ies";

        return new ApiResponse<CategoryAdminDto>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(AdminCategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count(), pluralSuffix),
        };
    }
}
