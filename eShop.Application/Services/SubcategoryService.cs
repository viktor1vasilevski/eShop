using eShop.Application.DTOs.Product;
using eShop.Application.DTOs.Subcategory;
using eShop.Application.Requests.Subcategory;

namespace eShop.Application.Services;

public class SubcategoryService(IUnitOfWork _uow) : ISubcategoryService
{
    private readonly IRepositoryBase<Subcategory> _subcategoryRepository = _uow.GetRepository<Subcategory>();
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();


    public ApiResponse<List<SubcategoryDTO>> GetSubcategories(SubcategoryRequest request)
    {
        var query = _subcategoryRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(!String.IsNullOrEmpty(request.CategoryId.ToString()), x => x.CategoryId == request.CategoryId)
                          .WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())).Where(x => !x.IsDeleted),
            include: x => x.Include(x => x.Category));

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

        var subcategoriesDTO = sortedQuery.Select(x => new SubcategoryDTO
        {
            Id = x.Id,
            Name = x.Name,
            Category = x.Category.Name,
            CategoryId = x.Category.Id,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<SubcategoryDTO>>()
        {
            Data = subcategoriesDTO,
            Status = ResponseStatus.Success,
            TotalCount = totalCount
        };
    }

    public async Task<ApiResponse<SubcategoryDetailsDTO>> GetSubcategoryByIdAsync(Guid id)
    {
        var subcategory = (await _subcategoryRepository.GetAsync(
            filter: x => x.Id == id && !x.IsDeleted,
            include: x => x.Include(x => x.Products).Include(x => x.Category)
            )).FirstOrDefault();

        if (subcategory is null)
            return new ApiResponse<SubcategoryDetailsDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
            };

        return new ApiResponse<SubcategoryDetailsDTO>
        {
            Status = ResponseStatus.Success,
            Data = new SubcategoryDetailsDTO()
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                CategoryId = subcategory.Category.Id,
                Category = subcategory.Category.Name,
                Products = subcategory.Products?.Select(x => new ProductRefDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToList()
            }
        };
    }

    public ApiResponse<SubcategoryDetailsDTO> CreateSubcategory(CreateUpdateSubcategoryRequest request)
    {
        var normalizedName = (request.Name ?? string.Empty).Trim();

        if (!_categoryRepository.Exists(x => x.Id == request.CategoryId))
            return new ApiResponse<SubcategoryDetailsDTO>()
            {
                Message = CategoryConstants.CategoryDoesNotExist,
                Status = ResponseStatus.NotFound,
            };

        if (_subcategoryRepository.Exists(x => x.Name.ToLower() == normalizedName))
            return new ApiResponse<SubcategoryDetailsDTO>()
            {
                Message = SubcategoryConstants.SUBCATEGORY_EXISTS,
                Status = ResponseStatus.Conflict
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var subcategory = Subcategory.Create(normalizedName, request.CategoryId, image);
            _subcategoryRepository.Insert(subcategory);
            _uow.SaveChanges();

            return new ApiResponse<SubcategoryDetailsDTO>
            {
                Status = ResponseStatus.Created,
                Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_CREATED,
                Data = new SubcategoryDetailsDTO
                {
                    Id = subcategory.Id,
                    Name = subcategory.Name,
                    CategoryId = subcategory.CategoryId,
                    Created = subcategory.Created
                }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<SubcategoryDetailsDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<SubcategoryDTO> DeleteSubcategory(Guid id)
    {
        var subcategory = _subcategoryRepository.GetAsQueryable(
                        filter: x => x.Id == id && x.Name != SystemConstants.UncategorizedSubcategoryName && !x.IsDeleted,
                        include: x => x.Include(x => x.Products)).FirstOrDefault();

        if (subcategory is null)
            return new ApiResponse<SubcategoryDTO>
            {
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
                Status = ResponseStatus.NotFound
            };


        if(subcategory.HasRelatedProducts())
            return new ApiResponse<SubcategoryDTO>
            {
                Message = SubcategoryConstants.SUBCATEGORY_HAS_RELATED_ENTITIES,
                Status = ResponseStatus.Conflict
            };

        subcategory.SoftDelete();
        _uow.SaveChanges();

        return new ApiResponse<SubcategoryDTO>
        {
            Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_DELETED,
            Status = ResponseStatus.Success
        };
    }

    public ApiResponse<SubcategoryDTO> UpdateSubcategory(Guid id, CreateUpdateSubcategoryRequest request)
    {
        var subcategory = _subcategoryRepository.GetById(id);
        if (subcategory is null)
            return new ApiResponse<SubcategoryDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST
            };

        if (!_categoryRepository.Exists(x => x.Id == request.CategoryId))
            return new ApiResponse<SubcategoryDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        var normalizedName = (request.Name ?? string.Empty).Trim();
        if (_subcategoryRepository.Exists(x => x.Name.ToLower() == normalizedName && x.Id != id))
            return new ApiResponse<SubcategoryDTO>
            {
                Status = ResponseStatus.Conflict,
                Message = SubcategoryConstants.SUBCATEGORY_EXISTS
            };

        try
        {
            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            subcategory.Update(normalizedName, request.CategoryId, image);
            _uow.SaveChanges();

            return new ApiResponse<SubcategoryDTO>
            {
                Status = ResponseStatus.Success,
                Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_EDITED,
                Data = new SubcategoryDTO { Id = id, Name = subcategory.Name }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<SubcategoryDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }

    }

    public async Task<ApiResponse<List<SelectSubcategoryListItemDTO>>> GetSubcategoriesDropdownListAsync()
    {
        var subcategories = await _subcategoryRepository.GetAsync(x => !x.IsDeleted);

        var subcategoriesDropdownDTO = subcategories.Select(x => new SelectSubcategoryListItemDTO
        {
            SubcategoryId = x.Id,
            Name = x.Name
        }).ToList();

        return new ApiResponse<List<SelectSubcategoryListItemDTO>>
        {
            Data = subcategoriesDropdownDTO
        };
    }

    public ApiResponse<List<SelectSubcategoryListItemDTO>> GetSubcategoriesWithCategoriesDropdownList()
    {
        var uncategorizedCategoryId = _categoryRepository
                        .Get(x => x.Name == SystemConstants.UncategorizedCategoryName && !x.IsDeleted)
                        .Select(x => x.Id)
                        .FirstOrDefault();

        var uncategorizedSubcategoryId = _subcategoryRepository
            .Get(x => x.Name == SystemConstants.UncategorizedSubcategoryName && x.CategoryId == uncategorizedCategoryId)
            .Select(x => x.Id)
            .FirstOrDefault();

        var subcategoriesDropdownDTO = _subcategoryRepository
            .Get(x => x.CategoryId != uncategorizedCategoryId || x.Id == uncategorizedSubcategoryId, null, x => x.Include(x => x.Category))
            .Select(x => new SelectSubcategoryListItemDTO
            {
                SubcategoryId = x.Id,
                Name = $"{x.Name} ({x.Category.Name})"
            })
            .ToList();

        return new ApiResponse<List<SelectSubcategoryListItemDTO>>
        {
            Data = subcategoriesDropdownDTO
        };
    }
}
