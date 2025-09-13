using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.DTOs.Product;
using eShop.Application.Interfaces.Category;
using eShop.Application.Requests.Category;

namespace eShop.Application.Services;

public class CategoryAdminService(IUnitOfWork _uow) : ICategoryAdminService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();


    // ovde sme dobri. Taman e i AdminCategoryDto. Gi ima bash potrebnite podatoci samo za tabelata
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



    // i ovde mislam deka sme dobri
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


    // ovde sme dobri, samo neznam dali da se koristi ova CategoryDto bidejki e od
    // normalnoto Dto, ne e od Admin. Sega za sega neka sedi
    public ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request)
    {
        try
        {
            var trimmedName = request.Name.Trim();
            if (_categoryRepository.Exists(x => x.Name.ToLower() == trimmedName.ToLower() && x.ParentCategoryId == request.ParentCategoryId && !x.IsDeleted))
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

}
