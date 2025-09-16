using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.DTOs.Product;
using eShop.Application.Interfaces.Category;
using eShop.Application.Requests.Category;

namespace eShop.Application.Services;

public class CategoryAdminService(IUnitOfWork _uow, ILogger<CategoryAdminService> _logger) : ICategoryAdminService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();


    public ApiResponse<List<AdminCategoryDto>> GetCategories(CategoryRequest request)
    {
        var query = _categoryRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(!string.IsNullOrEmpty(request.Name), c => c.Name.Contains(request.Name!, StringComparison.OrdinalIgnoreCase))
                          .WhereIf(true, x => !x.IsDeleted));

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

        var categoriesDTO = query.Select(c => new AdminCategoryDto
        {
            Id = c.Id,
            Name = Category.BuildFullName(c.Id, lookup),
            Created = c.Created,
            LastModified = c.LastModified,
        }).ToList();

        return new ApiResponse<List<AdminCategoryDto>>
        {
            Data = categoriesDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }

    public ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request)
    {
        try
        {
            var trimmedName = request.Name.Trim();
            var normalizedName = trimmedName.ToUpperInvariant();

            bool categoryExists = _categoryRepository.Exists(x =>
                x.Name.ToUpper() == normalizedName &&
                x.ParentCategoryId == request.ParentCategoryId &&
                !x.IsDeleted);

            if (categoryExists)
                return new ApiResponse<CategoryDto>
                {
                    Status = ResponseStatus.Conflict,
                    Message = CategoryConstants.CategoryExist
                };

            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Category.Create(trimmedName, image, request.ParentCategoryId);
            _categoryRepository.Insert(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDto>
            {
                Status = ResponseStatus.Created,
                Message = CategoryConstants.CategorySuccessfullyCreated,
                Data = new CategoryDto
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
            return new ApiResponse<CategoryDto>
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

    public ApiResponse<AdminCategoryDto> DeleteCategory(Guid id)
    {
        var allCategories = _categoryRepository
            .GetAsQueryable(c => !c.IsDeleted)
            .AsNoTracking()
            .ToList();

        var categoryToDelete = allCategories.FirstOrDefault(c => c.Id == id);
        if (categoryToDelete is null)
        {
            return new ApiResponse<AdminCategoryDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var idsToDelete = Category.GetDescendantIds(allCategories, id);

        var total = _productRepository
            .GetAsQueryable(p => idsToDelete.Contains(p.CategoryId))
            .Count();

        if (total > 0)
        {
            return new ApiResponse<AdminCategoryDto>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(CategoryConstants.CategoryHasProducts, total)
            };
        }

        var categoriesToDelete = _categoryRepository
            .GetAsQueryable(c => idsToDelete.Contains(c.Id))
            .ToList();

        Category.SoftDeleteRange(categoriesToDelete);

        _uow.SaveChanges();

        var pluralSuffix = categoriesToDelete.Count == 1 ? "y" : "ies";

        return new ApiResponse<AdminCategoryDto>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(CategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count, pluralSuffix),
        };
    }

    public ApiResponse<CategoryDto> UpdateCategory(Guid id, CreateUpdateCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id);
        if (category is null)
        {
            return new ApiResponse<CategoryDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var trimmedName = request.Name?.Trim() ?? string.Empty;
        var normalizedName = trimmedName.ToUpperInvariant();

        if (_categoryRepository.Exists(x => x.Id != id && x.ParentCategoryId == request.ParentCategoryId 
                && x.Name.ToUpper() == normalizedName && !x.IsDeleted))
        {
            return new ApiResponse<CategoryDto>
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
                return new ApiResponse<CategoryDto>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = CategoryConstants.CategoryCannotBeOwnParent
                };
            }

            if (request.ParentCategoryId.HasValue)
            {
                var all = _categoryRepository.GetAsQueryable(c => !c.IsDeleted).AsNoTracking().ToList();
                var descendants = Category.GetDescendantIds(all, id);
                if (descendants.Contains(request.ParentCategoryId.Value))
                {
                    return new ApiResponse<CategoryDto>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = CategoryConstants.CategoryCannotBeMovedUnderDescendant
                    };
                }
            }

            category.Update(trimmedName, image, request.ParentCategoryId);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDto>
            {
                Status = ResponseStatus.Success,
                Message = CategoryConstants.CategorySuccessfullyUpdated,
                Data = new CategoryDto
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
            return new ApiResponse<CategoryDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<AdminCategoryDetailsDto>> GetCategoryByIdAsync(Guid id)
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
            return new ApiResponse<AdminCategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var dto = new AdminCategoryDetailsDto
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

        return new ApiResponse<AdminCategoryDetailsDto>
        {
            Status = ResponseStatus.Success,
            Data = dto
        };
    }

    public async Task<ApiResponse<CategoryEditDto>> GetCategoryForEditAsync(Guid id)
    {
        var category = await _categoryRepository
            .GetAsQueryable(x => x.Id == id && !x.IsDeleted)
            .Select(c => new CategoryEditDto
            {
                Id = c.Id,
                Name = c.Name,
                ParentCategoryId = c.ParentCategoryId,
                Image = ImageDataUriBuilder.FromImage(c.Image),
                Children = c.Children.Select(ch => new CategoryRefDto
                {
                    Id = ch.Id,
                    Name = ch.Name
                }).ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();


        if (category is null)
        {
            return new ApiResponse<CategoryEditDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        return new ApiResponse<CategoryEditDto>
        {
            Status = ResponseStatus.Success,
            Data = category
        };
    }

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync()
    {
        var allCategories = (await _categoryRepository.GetAsync(x => !x.IsDeleted)).ToList();
        var tree = BuildCategoryTree(allCategories);

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }




    #region private methods

    private List<CategoryTreeDto> BuildCategoryTree(List<Category> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryTreeDto
            {
                Id = c.Id,
                Name = c.Name,
                Children = BuildCategoryTree(categories, c.Id)
            })
            .ToList();
    }

    private List<Guid> GetAllDescendantCategoryIds(IEnumerable<Category> allCategories, Guid parentId)
    {
        var result = new List<Guid> { parentId };
        foreach (var child in allCategories.Where(c => c.ParentCategoryId == parentId))
        {
            result.AddRange(GetAllDescendantCategoryIds(allCategories, child.Id));
        }
        return result;
    }

    #endregion

}
