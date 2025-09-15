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
        var allCategories = _categoryRepository.GetAsQueryable(c => !c.IsDeleted).AsNoTracking().ToList();

        var filtered = allCategories.Where(c => string.IsNullOrEmpty(request.Name) || c.Name.Contains(request.Name, StringComparison.OrdinalIgnoreCase)).ToList();

        var totalCount = filtered.Count;

        var sortKeySelector = new Dictionary<string, Func<Category, object>>(StringComparer.OrdinalIgnoreCase)
        {
            ["created"] = c => c.Created,
            ["lastmodified"] = c => c.LastModified
        };

        if (!string.IsNullOrEmpty(request.SortBy) &&
            sortKeySelector.TryGetValue(request.SortBy, out var keySelector))
        {
            filtered = (request.SortDirection?.ToLower()) switch
            {
                "desc" => filtered.OrderByDescending(keySelector).ToList(),
                _ => filtered.OrderBy(keySelector).ToList()
            };
        }

        if (request.Skip.HasValue)
            filtered = filtered.Skip(request.Skip.Value).ToList();

        if (request.Take.HasValue)
            filtered = filtered.Take(request.Take.Value).ToList();

        var lookup = allCategories.ToDictionary(c => c.Id);

        var categoriesDTO = filtered.Select(c => new AdminCategoryDto
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

    public async Task<ApiResponse<AdminCategoryDetailsDto>> GetCategoryByIdAsync(Guid id)
    {
        var category = (await _categoryRepository.GetAsync(
            filter: x => x.Id == id && !x.IsDeleted,
            include: x => x
                .Include(x => x.Products)
                .Include(x => x.Children)))
            ?.FirstOrDefault();

        if (category is null)
            return new ApiResponse<AdminCategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        var categoryDetaislDto = new AdminCategoryDetailsDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = ImageDataUriBuilder.FromImage(category.Image),
            ParentCategoryId = category.ParentCategoryId,
            Created = category.Created,
            LastModified = category.LastModified,
            Products = category.Products?
                .Select(p => new ProductRefDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList() ?? new List<ProductRefDto>(),
            Children = category.Children?
                .Select(c => new CategoryRefDto
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList() ?? new List<CategoryRefDto>()
        };

        return new ApiResponse<AdminCategoryDetailsDto>
        {
            Status = ResponseStatus.Success,
            Data = categoryDetaislDto
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


    public ApiResponse<CategoryDetailsDto> DeleteCategory(Guid id)
    {
        var allCategories = _categoryRepository
            .GetAsQueryable(c => !c.IsDeleted)
            .AsNoTracking()
            .ToList();

        var categoryToDelete = allCategories.FirstOrDefault(c => c.Id == id);
        if (categoryToDelete is null)
        {
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var idsToDelete = Category.GetDescendantIds(allCategories, id);

        if (_productRepository.Exists(p => idsToDelete.Contains(p.CategoryId)))
        {
            var total = _productRepository
                .GetAsQueryable(p => idsToDelete.Contains(p.CategoryId))
                .Count();

            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.Conflict,
                Message = string.Format(CategoryConstants.CategoryHasProducts, total)
            };
        }

        var categoriesToDelete = _categoryRepository
            .GetAsQueryable(c => idsToDelete.Contains(c.Id))
            .ToList();

        foreach (var cat in categoriesToDelete)
        {
            cat.SoftDelete();
        }

        _uow.SaveChanges();

        var pluralSuffix = categoriesToDelete.Count == 1 ? "y" : "ies";

        return new ApiResponse<CategoryDetailsDto>
        {
            Status = ResponseStatus.Success,
            Message = string.Format(CategoryConstants.CategoriesDeletedMessage, categoriesToDelete.Count, pluralSuffix)
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

    public ApiResponse<CategoryDetailsDto> UpdateCategory(Guid id, CreateUpdateCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id);
        if (category is null)
        {
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };
        }

        var trimmedName = request.Name?.Trim() ?? string.Empty;

        if (_categoryRepository.Exists(x => x.Name.ToLower() == trimmedName.ToLower() && x.Id != id))
        {
            return new ApiResponse<CategoryDetailsDto>
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

            // Prevent setting parent to itself
            if (request.ParentCategoryId.HasValue && request.ParentCategoryId == id)
            {
                return new ApiResponse<CategoryDetailsDto>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = "A category cannot be its own parent."
                };
            }

            category.Update(trimmedName, image, request.ParentCategoryId);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_UPDATE,
                Data = new CategoryDetailsDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Image = ImageDataUriBuilder.FromImage(category.Image),
                    Created = category.Created,
                    LastModified = category.LastModified
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public async Task<ApiResponse<CategoryEditDto>> GetCategoryForEditAsync(Guid id)
    {
        var category = await _categoryRepository
            .GetAsQueryable(x => x.Id == id && !x.IsDeleted)
            .Include(x => x.Children)
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

    #region private methods

    private List<CategoryTreeDto> BuildCategoryTree(List<Category> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
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
