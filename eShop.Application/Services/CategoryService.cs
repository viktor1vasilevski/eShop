using eShop.Application.Constants;
using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Subcategory;
using eShop.Application.Enums;
using eShop.Application.Extensions;
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
    private readonly IRepositoryBase<Subcategory> _subcategoryRepository = _uow.GetRepository<Subcategory>();



    public ApiResponse<List<CategoryDetailsDTO>> GetCategories(CategoryRequest request)
    {
        var categories = _categoryRepository.GetAsQueryableWhereIf(
                        filter: x => x.WhereIf(!String.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())));

        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                categories = request.SortBy.ToLower() switch
                {
                    "created" => categories.OrderBy(x => x.Created),
                    "lastmodified" => categories.OrderBy(x => x.LastModified),
                    _ => categories.OrderBy(x => x.Created)
                };
            }
            else if (request.SortDirection.ToLower() == "desc")
            {
                categories = request.SortBy.ToLower() switch
                {
                    "created" => categories.OrderByDescending(x => x.Created),
                    "lastmodified" => categories.OrderByDescending(x => x.LastModified),
                    _ => categories.OrderByDescending(x => x.Created)
                };
            }
        }

        var totalCount = categories.Count();

        if (request.Skip.HasValue)
            categories = categories.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            categories = categories.Take(request.Take.Value);

        var categoriesDTO = categories.Select(x => new CategoryDetailsDTO
        {
            Id = x.Id,
            Name = x.Name,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<CategoryDetailsDTO>>()
        {
            Data = categoriesDTO,
            TotalCount = totalCount,
            NotificationType = NotificationType.Success,
        };
    }

    public ApiResponse<CategoryDetailsDTO> CreateCategory(CreateUpdateCategoryRequest request)
    {
        if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower()))
            return new ApiResponse<CategoryDetailsDTO>()
            {
                Message = CategoryConstants.CATEGORY_EXISTS,
                NotificationType = NotificationType.Conflict
            };

        try
        {
            var category = Category.CreateNew(request.Name);
            _categoryRepository.Insert(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDTO>
            {
                NotificationType = NotificationType.Created,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_CREATED,
                Data = new CategoryDetailsDTO { Id = category.Id, Name = category.Name, Created = category.Created }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryDetailsDTO>
            {
                NotificationType = NotificationType.BadRequest,
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
                NotificationType = NotificationType.NotFound,
                Message = CategoryConstants.CATEGORY_DOESNT_EXIST
            };

        if (_categoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id))
            return new ApiResponse<CategoryDetailsDTO>
            {
                NotificationType = NotificationType.Conflict,
                Message = CategoryConstants.CATEGORY_EXISTS
            };

        try
        {
            category.Update(request.Name);
            _categoryRepository.Update(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryDetailsDTO>
            {
                NotificationType = NotificationType.Success,
                Message = CategoryConstants.CATEGORY_SUCCESSFULLY_UPDATE,
                Data = new CategoryDetailsDTO { Id = id, Name = category.Name }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<CategoryDetailsDTO>
            {
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<CategoryDetailsDTO> DeleteCategory(Guid id)
    {
        var category = _categoryRepository.GetAsQueryable(
                         filter: x => x.Id == id && x.Name != "UNCATEGORIZED",
                         include: x => x.Include(x => x.Subcategories).ThenInclude(x => x.Products)
                         ).FirstOrDefault();

        if (category is null)
            return new ApiResponse<CategoryDetailsDTO>
            {
                Message = CategoryConstants.CATEGORY_DOESNT_EXIST,
                NotificationType = NotificationType.NotFound
            };

        if (HasRelatedEntities(category))
            return new ApiResponse<CategoryDetailsDTO>
            {
                Message = CategoryConstants.CATEGORY_HAS_RELATED_ENTITIES,
                NotificationType = NotificationType.Conflict
            };

        _categoryRepository.Delete(category);
        _uow.SaveChanges();

        return new ApiResponse<CategoryDetailsDTO>
        {
            Message = CategoryConstants.CATEGORY_SUCCESSFULLY_DELETED,
            NotificationType = NotificationType.Success
        };
    }

    public ApiResponse<List<SelectCategoryListItemDTO>> GetCategoriesDropdownList()
    {
        var categories = _categoryRepository.GetAsQueryable();

        var categoriesDropdownDTO = categories.Select(x => new SelectCategoryListItemDTO
        {
            Id = x.Id,
            Name = x.Name
        }).ToList();

        return new ApiResponse<List<SelectCategoryListItemDTO>>
        {
            Data = categoriesDropdownDTO
        };
    }

    public ApiResponse<List<CategoryWithSubcategoriesDTO>> GetCategoriesWithSubcategoriesForMenu()
    {
        var uncategorizedCategoryId = _categoryRepository
            .Get(x => x.Name == "UNCATEGORIZED")
            .Select(x => x.Id)
            .FirstOrDefault();

        var uncategorizedSubcategoryId = _subcategoryRepository
            .Get(x => x.Name == "UNCATEGORIZED" && x.CategoryId == uncategorizedCategoryId)
            .Select(x => x.Id)
            .FirstOrDefault();

        // Fetch all categories with subcategories and their products
        var categories = _categoryRepository.Get(
            filter: x => x.Id != uncategorizedCategoryId,
            include: x => x.Include(c => c.Subcategories)
                           .ThenInclude(sc => sc.Products)
        );

        var categoriesDto = categories
            .Select(c => new CategoryWithSubcategoriesDTO
            {
                Id = c.Id,
                Name = c.Name,
                Subcategories = c.Subcategories
                    .Where(sc =>
                        !string.Equals(sc.Name, "UNCATEGORIZED", StringComparison.OrdinalIgnoreCase) &&
                        sc.Id != uncategorizedSubcategoryId &&
                        sc.Products != null &&
                        sc.Products.Any()
                    )
                    .Select(sc => new SelectSubcategoryListItemDTO
                    {
                        Id = sc.Id,
                        Name = sc.Name
                    })
                    .ToList()
            })
            .Where(c => c.Subcategories.Any()) // Only keep categories that have valid subcategories
            .ToList();

        return new ApiResponse<List<CategoryWithSubcategoriesDTO>>
        {
            Data = categoriesDto
        };
    }



    private bool HasRelatedEntities(Category category)
    {
        return category.Subcategories?.Any() == true ||
               category.Subcategories?.FirstOrDefault()?.Products?.Any() == true;
    }

    public ApiResponse<CategoryDTO> GetCategoryById(Guid id)
    {
        var category = _categoryRepository.GetById(id);

        if (category is null)
            return new ApiResponse<CategoryDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = CategoryConstants.CATEGORY_DOESNT_EXIST
            };

        return new ApiResponse<CategoryDTO>
        {
            NotificationType = NotificationType.Success,
            Data = new CategoryDTO { Id = category.Id, Name = category.Name }
        };
    }
}
