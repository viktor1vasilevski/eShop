using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.DTOs.Product;
using eShop.Application.Interfaces.Category;
using eShop.Application.Requests.Category;

namespace eShop.Application.Services;

public class CategoryAdminService(IUnitOfWork _uow) : ICategoryAdminService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();

    public ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request)
    {
        var normalizedName = (request.Name ?? string.Empty).Trim();
        //if (_categoryRepository.Exists(x => x.Name.ToLower() == normalizedName.ToLower()))
        //    return new ApiResponse<CategoryDto>
        //    {
        //        Message = CategoryConstants.CategoryExist,
        //        Status = ResponseStatus.Conflict
        //    };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Category.Create(normalizedName, image, request.ParentCategoryId);
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
    }

    public ApiResponse<CategoryDetailsDto> DeleteCategory(Guid id)
    {
        //var category = _categoryRepository.GetAsQueryable(
        //      filter: x => x.Id == id && !x.IsDeleted && x.Name != SystemConstants.UncategorizedCategoryName,
        //      include: x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)).FirstOrDefault();

        //if (category is null)
        //    return new ApiResponse<CategoryDetailsDTO>
        //    {
        //        Message = CategoryConstants.CategoryDoesNotExist,
        //        Status = ResponseStatus.NotFound
        //    };

        //if(category.HasRelatedSubcategories())
        //    return new ApiResponse<CategoryDetailsDTO>
        //    {
        //        Message = CategoryConstants.CATEGORY_HAS_RELATED_ENTITIES,
        //        Status = ResponseStatus.Conflict
        //    };

        //category.SoftDelete();
        //_uow.SaveChanges();

        return new ApiResponse<CategoryDetailsDto>
        {
            Message = CategoryConstants.CATEGORY_SUCCESSFULLY_DELETED,
            Status = ResponseStatus.Success
        };
    }

    public ApiResponse<List<AdminCategoryDto>> GetCategories(CategoryRequest request)
    {
        var query = _categoryRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(true, x => !x.IsDeleted))
                          .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower()));

        var totalCount = query.Count();

        var sortedQuery = query;
        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderBy(x => x.Created),
                    "lastmodified" => sortedQuery.OrderBy(x => x.LastModified),
                    _ => sortedQuery.OrderBy(x => x.Created)
                };
            }
            else if (request.SortDirection.ToLower() == "desc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderByDescending(x => x.Created),
                    "lastmodified" => sortedQuery.OrderByDescending(x => x.LastModified),
                    _ => sortedQuery.OrderByDescending(x => x.Created)
                };
            }
        }

        if (request.Skip.HasValue)
            sortedQuery = sortedQuery.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            sortedQuery = sortedQuery.Take(request.Take.Value);

        var categoriesDTO = sortedQuery.Select(x => new AdminCategoryDto
        {
            Id = x.Id,
            Name = x.Name,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<AdminCategoryDto>>()
        {
            Data = categoriesDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success,
        };
    }

    public async Task<ApiResponse<CategoryDetailsDto>> GetCategoryByIdAsync(Guid id)
    {
        var category = (await _categoryRepository.GetAsync(
            filter: x => x.Id == id && !x.IsDeleted,
            include: x => x
                .Include(x => x.Products)
                .Include(x => x.Children)))
            ?.FirstOrDefault();

        if (category is null)
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        var categoryDetaislDto = new CategoryDetailsDto
        {
            Id = category.Id,
            Name = category.Name,
            Image = ImageDataUriBuilder.FromImage(category.Image),
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

        return new ApiResponse<CategoryDetailsDto>
        {
            Status = ResponseStatus.Success,
            Data = categoryDetaislDto
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
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id))
            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.Conflict,
                Message = CategoryConstants.CategoryExist
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            category.Update(request.Name, image, request.ParentCategoryId);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDto>
            {
                Status = ResponseStatus.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_UPDATE,
                Data = new CategoryDetailsDto { Id = id, Name = category.Name }
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
}
