using eShop.Application.Constants;
using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Product;
using eShop.Application.DTOs.Subcategory;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Category;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services;

public class CategoryService(IUnitOfWork _uow) : ICategoryService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();

    public ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request)
    {
        var query = _categoryRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())).Where(x => !x.IsDeleted));

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

        var categoriesDTO = sortedQuery.Select(x => new CategoryDTO
        {
            Id = x.Id,
            Name = x.Name,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<CategoryDTO>>()
        {
            Data = categoriesDTO,
            TotalCount = totalCount,
            Status = ResponseStatus.Success,
        };
    }

    public async Task<ApiResponse<CategoryDetailsDTO>> GetCategoryByIdAsync(Guid id)
    {
        var category = (await _categoryRepository.GetAsync(
            filter: x => x.Id == id && !x.IsDeleted,
            include: x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)))?.FirstOrDefault();

        if (category is null)
            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        return new ApiResponse<CategoryDetailsDTO>
        {
            Status = ResponseStatus.Success,
            Data = new CategoryDetailsDTO
            {
                Id = category.Id,
                Name = category.Name,
                Created = category.Created,
                LastModified = category.LastModified,
                Image = ImageHelper.BuildImageDataUrl(category.Image, category.ImageType),
                Subcategories = category.Subcategories
                    .Select(sc => new SubcategoryRefDTO
                    {
                        Id = sc.Id,
                        Name = sc.Name,
                        Products = sc.Products
                            .Select(p => new ProductRefDTO
                            {
                                Id = p.Id,
                                Name = p.Name
                            }).ToList()
                    }).ToList()
            }
        };

    }

    public ApiResponse<CategoryDetailsDTO> CreateCategory(CreateUpdateCategoryRequest request)
    {
        if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower()))
            return new ApiResponse<CategoryDetailsDTO>()
            {
                Message = CategoryConstants.CATEGORY_EXISTS,
                Status = ResponseStatus.Conflict
            };

        try
        {
            var category = Category.Create(request.Name, request.Image);
            _categoryRepository.Insert(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.Created,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_CREATED,
                Data = new CategoryDetailsDTO { Id = category.Id, Name = category.Name, Created = category.Created }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<CategoryDetailsDTO> UpdateCategory(Guid id, CreateUpdateCategoryRequest request)
    {
        var category = _categoryRepository.GetById(id);
        if (category is null)
            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.NotFound,
                Message = CategoryConstants.CategoryDoesNotExist
            };

        if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id))
            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.Conflict,
                Message = CategoryConstants.CATEGORY_EXISTS
            };

        try
        {
            category.Update(request.Name, request.Image);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_UPDATE,
                Data = new CategoryDetailsDTO { Id = id, Name = category.Name }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryDetailsDTO>
            {
                Status = ResponseStatus.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<CategoryDetailsDTO> DeleteCategory(Guid id)
    {
        var category = _categoryRepository.GetAsQueryable(
              filter: x => x.Id == id && !x.IsDeleted && x.Name != SystemConstants.UNCATEGORIZED_CATEGORY_NAME,
              include: x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)).FirstOrDefault();

        if (category is null)
            return new ApiResponse<CategoryDetailsDTO>
            {
                Message = CategoryConstants.CategoryDoesNotExist,
                Status = ResponseStatus.NotFound
            };

        if(category.HasRelatedSubcategories())
            return new ApiResponse<CategoryDetailsDTO>
            {
                Message = CategoryConstants.CATEGORY_HAS_RELATED_ENTITIES,
                Status = ResponseStatus.Conflict
            };

        category.SoftDelete();
        _uow.SaveChanges();

        return new ApiResponse<CategoryDetailsDTO>
        {
            Message = CategoryConstants.CATEGORY_SUCCESSFULLY_DELETED,
            Status = ResponseStatus.Success
        };
    }

    public async Task<ApiResponse<List<SelectCategoryListItemDTO>>> GetCategoriesDropdownListAsync()
    {
        var categories = await _categoryRepository.GetAsync(x => !x.IsDeleted);

        var categoriesDropdownDTO = categories.Select(x => new SelectCategoryListItemDTO
        {
            CategoryId = x.Id,
            Name = x.Name
        }).ToList();

        return new ApiResponse<List<SelectCategoryListItemDTO>>
        {
            Data = categoriesDropdownDTO
        };
    }



    public ApiResponse<List<CategoryWithSubcategoriesDTO>> GetCategoriesWithSubcategoriesForMenu()
    {
        var result = _categoryRepository
            .GetAsQueryable(
                filter: c => c.Name != SystemConstants.UNCATEGORIZED_CATEGORY_NAME && !c.IsDeleted
            )
            .Select(c => new
            {
                Category = c,
                ValidSubcategories = c.Subcategories
                    .Where(sc => sc.Name != SystemConstants.UNCATEGORIZED_SUBCATEGORY_NAME && !sc.IsDeleted && sc.Products.Any())
                    .Select(sc => new { sc.Id, sc.Name })
                    .ToList()
            })
            .Where(x => x.ValidSubcategories.Any())
            .Select(x => new CategoryWithSubcategoriesDTO
            {
                CategoryId = x.Category.Id,
                Name = x.Category.Name,
                Subcategories = x.ValidSubcategories
                    .Select(sc => new SelectSubcategoryListItemDTO
                    {
                        SubcategoryId = sc.Id,
                        Name = sc.Name
                    }).ToList()
            }).ToList();

        return new ApiResponse<List<CategoryWithSubcategoriesDTO>>
        {
            Data = result
        };
    }
}
