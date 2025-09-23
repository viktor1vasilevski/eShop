using eShop.Application.DTOs.Admin.Category;
using eShop.Application.DTOs.Product;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Requests.Category;
using eShop.Application.Responses.Admin.Category;
using static eShop.Domain.Entities.Category;

namespace eShop.Application.Services;

public class CategoryAdminService(IUnitOfWork _uow, ILogger<CategoryAdminService> _logger) : ICategoryAdminService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();


    public ApiResponse<List<CategoryAdminDto>> GetCategories(CategoryAdminRequest request)
    {
        var query = _categoryRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(true, x => !x.IsDeleted)
                          .WhereIf(!string.IsNullOrEmpty(request.Name), c => c.Name.Contains(request.Name!, StringComparison.OrdinalIgnoreCase)));

        var totalCount = query.Count();

        var sortBy = request.SortBy?.Trim().ToLower();
        var sortDirection = request.SortDirection?.Trim().ToLower();

        if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortDirection))
        {
            var sortMap = new Dictionary<string, Func<IQueryable<Category>, IOrderedQueryable<Category>>>
            {
                ["created"] = q => sortDirection == "asc" ? q.OrderBy(x => x.Created) : q.OrderByDescending(x => x.Created),
                ["lastmodified"] = q => sortDirection == "asc" ? q.OrderBy(x => x.LastModified) : q.OrderByDescending(x => x.LastModified)
            };

            query = sortMap.TryGetValue(sortBy, out var sorter)
                ? sorter(query)
                : (sortDirection == "asc" ? query.OrderBy(x => x.Created) : query.OrderByDescending(x => x.Created));
        }

        if (request.Skip.HasValue)
            query = query.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            query = query.Take(request.Take.Value);

        var lookup = query.ToDictionary(c => c.Id);

        var categoriesDTO = query.Select(c => new CategoryAdminDto
        {
            Id = c.Id,
            Name = Category.BuildFullName(c.Id, lookup),
            Created = c.Created,
            LastModified = c.LastModified,
        }).ToList();

        return new ApiResponse<List<CategoryAdminDto>>
        {
            Data = categoriesDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public ApiResponse<CategoryAdminDto> CreateCategory(CreateUpdateCategoryRequest request)
    {
        try
        {
            var trimmedName = request.Name.Trim();
            var normalizedName = trimmedName.ToLower();

            if (_categoryRepository.Exists(x =>
                x.Name.ToLower() == normalizedName &&
                x.ParentCategoryId == request.ParentCategoryId &&
                !x.IsDeleted))
                return new ApiResponse<CategoryAdminDto>
                {
                    Status = ResponseStatus.Conflict,
                    Message = CategoryConstants.CategoryExist
                };

            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Create(trimmedName, image, request.ParentCategoryId);
            _categoryRepository.Insert(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Created,
                Message = CategoryConstants.CategorySuccessfullyCreated,
                Data = new CategoryAdminDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    ParentCategoryId = category.ParentCategoryId,
                    Created = category.Created
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

    public ApiResponse<CategoryAdminDto> DeleteCategory(Guid id)
    {
        if (!_categoryRepository.Exists(c => !c.IsDeleted && c.Id == id))
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var allCategories = _categoryRepository
            .GetAsQueryable(c => !c.IsDeleted)
            .AsNoTracking()
            .Select(c => new CategoryNode(c.Id, c.ParentCategoryId))
            .ToList();

        var idsToDelete = Category.GetDescendantIds(allCategories, id);

        var productCount = _productRepository
            .GetAsQueryable(p => idsToDelete.Contains(p.CategoryId))
            .Count();

        if (productCount > 0)
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(CategoryConstants.CategoryHasProducts, productCount)
            };
        }

        var categoriesToDelete = _categoryRepository
            .GetAsQueryable(c => idsToDelete.Contains(c.Id))
            .ToList();

        SoftDeleteRange(categoriesToDelete);

        _uow.SaveChanges();

        var pluralSuffix = categoriesToDelete.Count == 1 ? "y" : "ies";

        return new ApiResponse<CategoryAdminDto>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(CategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count, pluralSuffix),
        };
    }

    public async Task<ApiResponse<CategoryAdminDto>> UpdateCategory(Guid id, CreateUpdateCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id);
        if (category is null)
        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var normalizedName = trimmedName.ToLower();

        if (await _categoryRepository.ExistsAsync(x =>
            x.Id != id &&
            x.ParentCategoryId == request.ParentCategoryId &&
            x.Name.ToLower() == normalizedName &&
            !x.IsDeleted))

        {
            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Conflict,
                Message = CategoryConstants.CategoryExist
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
                    Message = CategoryConstants.CategoryCannotBeOwnParent
                };
            }

            if (request.ParentCategoryId.HasValue)
            {
                var all = _categoryRepository.GetAsQueryable(c => !c.IsDeleted)
                    .Select(c => new CategoryNode(c.Id, c.ParentCategoryId)).AsNoTracking().ToList();
                var descendants = Category.GetDescendantIds(all, id);
                if (descendants.Contains(request.ParentCategoryId.Value))
                {
                    return new ApiResponse<CategoryAdminDto>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = CategoryConstants.CategoryCannotBeMovedUnderDescendant
                    };
                }
            }

            category.Update(trimmedName, image, request.ParentCategoryId);
            await _uow.SaveChangesAsync();

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Success,
                Message = CategoryConstants.CategorySuccessfullyUpdated,
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
        var category = await _categoryRepository
            .GetAsQueryable(c => c.Id == id && !c.IsDeleted,
                include: q => q
                    .Include(c => c.Products)
                    .Include(c => c.Children))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (category is null)
        {
            return new ApiResponse<CategoryDetailsAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var dto = new CategoryDetailsAdminDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = ImageDataUriBuilder.FromImage(category.Image),
            ParentCategoryId = category.ParentCategoryId,
            Created = category.Created,
            LastModified = category.LastModified,
            Products = category.Products?
                .OrderBy(p => p.Name)
                .Select(p => new ProductRefDto { Id = p.Id, Name = p.Name })
                .ToList() ?? new(),
            Children = category.Children?
                .OrderBy(p => p.Name)
                .Select(c => new CategoryRefDto { Id = c.Id, Name = c.Name })
                .ToList() ?? new()
        };

        return new ApiResponse<CategoryDetailsAdminDto>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    public async Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id)
    {
        var allCategories = await _categoryRepository
            .GetAsQueryable(x => !x.IsDeleted)
            .Select(c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        var entity = await _categoryRepository
            .GetAsQueryable(x => x.Id == id && !x.IsDeleted)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return new ApiResponse<CategoryEditAdminDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }


        var excludedIds = new HashSet<Guid>();
        CollectDescendantIds(allCategories.First(c => c.Id == id), allCategories, excludedIds);
        excludedIds.Add(id);


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


    // helper to collect descendants
    private void CollectDescendantIds(CategoryFlatDto category, List<CategoryFlatDto> all, HashSet<Guid> ids)
    {
        var children = all.Where(c => c.ParentCategoryId == category.Id);
        foreach (var child in children)
        {
            ids.Add(child.Id);
            CollectDescendantIds(child, all, ids);
        }
    }

    private List<Guid> GetDescendantIds(Category category)
    {
        var ids = new List<Guid>();

        if (category.Children == null)
            return ids;

        foreach (var child in category.Children)
        {
            ids.Add(child.Id);
            ids.AddRange(GetDescendantIds(child));
        }

        return ids;
    }

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync()
    {
        var allCategories = await _categoryRepository
            .GetAsQueryable(x => !x.IsDeleted)
            .Select(c => new CategoryFlatDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                ProductCount = c.Products.Count
            })
            .ToListAsync();

        var tree = BuildCategoryTree(allCategories);

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }


    #region private methods

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

    #endregion

}
