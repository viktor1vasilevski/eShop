using eShop.Application.Constants;
using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Subcategory;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Subcategory;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services;

public class SubcategoryService(IUnitOfWork _uow) : ISubcategoryService
{
    private readonly IRepositoryBase<Subcategory> _subcategoryRepository = _uow.GetRepository<Subcategory>();
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();

    public ApiResponse<SubcategoryDetailsDTO> CreateSubcategory(CreateUpdateSubcategoryRequest request)
    {
        if (!_categoryRepository.Exists(x => x.Id == request.CategoryId))
            return new ApiResponse<SubcategoryDetailsDTO>()
            {
                Message = CategoryConstants.CATEGORY_DOESNT_EXIST,
                NotificationType = NotificationType.NotFound,
            };

        if (_subcategoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower()))
            return new ApiResponse<SubcategoryDetailsDTO>()
            {
                Message = SubcategoryConstants.SUBCATEGORY_EXISTS,
                NotificationType = NotificationType.Conflict
            };

        try
        {
            var subcategory = Subcategory.CreateNew(request.CategoryId, request.Name);
            _subcategoryRepository.Insert(subcategory);
            _uow.SaveChanges();

            return new ApiResponse<SubcategoryDetailsDTO>
            {
                NotificationType = NotificationType.Created,
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
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }
    }

    public ApiResponse<SubcategoryDTO> DeleteSubcategory(Guid id)
    {
        var subcategory = _subcategoryRepository.GetAsQueryable(
                        filter: x => x.Id == id && x.Name != SystemConstants.UNCATEGORIZED_SUBCATEGORY_NAME && !x.IsDeleted,
                        include: x => x.Include(x => x.Products)).FirstOrDefault();

        if (subcategory is null)
            return new ApiResponse<SubcategoryDTO>
            {
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
                NotificationType = NotificationType.NotFound
            };


        if(subcategory.HasRelatedProducts())
            return new ApiResponse<SubcategoryDTO>
            {
                Message = SubcategoryConstants.SUBCATEGORY_HAS_RELATED_ENTITIES,
                NotificationType = NotificationType.Conflict
            };

        subcategory.SoftDelete();
        _uow.SaveChanges();

        return new ApiResponse<SubcategoryDTO>
        {
            Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_DELETED,
            NotificationType = NotificationType.Success
        };
    }

    public ApiResponse<SubcategoryDTO> UpdateSubcategory(Guid id, CreateUpdateSubcategoryRequest request)
    {
        var subcategory = _subcategoryRepository.GetById(id);
        if (subcategory is null)
            return new ApiResponse<SubcategoryDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST
            };

        if (!_categoryRepository.Exists(x => x.Id == request.CategoryId))
            return new ApiResponse<SubcategoryDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = CategoryConstants.CATEGORY_DOESNT_EXIST
            };

        if (_subcategoryRepository.Exists(x => x.Name.ToLower() == request.Name.ToLower() && x.Id != id))
            return new ApiResponse<SubcategoryDTO>
            {
                NotificationType = NotificationType.Conflict,
                Message = SubcategoryConstants.SUBCATEGORY_EXISTS
            };

        try
        {
            subcategory.Update(request.CategoryId, request.Name);
            _uow.SaveChanges();

            return new ApiResponse<SubcategoryDTO>
            {
                NotificationType = NotificationType.Success,
                Message = SubcategoryConstants.SUBCATEGORY_SUCCESSFULLY_EDITED,
                Data = new SubcategoryDTO { Id = id, Name = subcategory.Name }
            };
        }
        catch (DomainValidationException ex)
        {
            return new ApiResponse<SubcategoryDTO>
            {
                NotificationType = NotificationType.BadRequest,
                Message = ex.Message
            };
        }

    }

    public ApiResponse<List<SubcategoryDetailsDTO>> GetSubcategories(SubcategoryRequest request)
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

        var subcategoriesDTO = sortedQuery.Select(x => new SubcategoryDetailsDTO
        {
            Id = x.Id,
            Name = x.Name,
            Category = x.Category.Name,
            CategoryId = x.Category.Id,
            Created = x.Created,
            LastModified = x.LastModified,
        }).ToList();

        return new ApiResponse<List<SubcategoryDetailsDTO>>()
        {
            Data = subcategoriesDTO,
            NotificationType = NotificationType.Success,
            TotalCount = totalCount
        };
    }

    public async Task<ApiResponse<List<SelectSubcategoryListItemDTO>>> GetSubcategoriesDropdownListAsync()
    {
        var subcategories = await _subcategoryRepository.GetAsync(x => !x.IsDeleted);

        var subcategoriesDropdownDTO = subcategories.Select(x => new SelectSubcategoryListItemDTO
        {
            Id = x.Id,
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
                        .Get(x => x.Name == SystemConstants.UNCATEGORIZED_CATEGORY_NAME && !x.IsDeleted)
                        .Select(x => x.Id)
                        .FirstOrDefault();

        var uncategorizedSubcategoryId = _subcategoryRepository
            .Get(x => x.Name == SystemConstants.UNCATEGORIZED_SUBCATEGORY_NAME && x.CategoryId == uncategorizedCategoryId)
            .Select(x => x.Id)
            .FirstOrDefault();

        var subcategoriesDropdownDTO = _subcategoryRepository
            .Get(x => x.CategoryId != uncategorizedCategoryId || x.Id == uncategorizedSubcategoryId, null, x => x.Include(x => x.Category))
            .Select(x => new SelectSubcategoryListItemDTO
            {
                Id = x.Id,
                Name = $"{x.Name} ({x.Category.Name})"
            })
            .ToList();

        return new ApiResponse<List<SelectSubcategoryListItemDTO>>
        {
            Data = subcategoriesDropdownDTO
        };
    }

    public ApiResponse<SubcategoryDetailsDTO> GetSubcategoryById(Guid id)
    {
        var subcategory = _subcategoryRepository.GetAsQueryable(x => x.Id == id && !x.IsDeleted, null,
            x => x.Include(x => x.Products).Include(x => x.Category)).FirstOrDefault();

        if(subcategory is null)
            return new ApiResponse<SubcategoryDetailsDTO>
            {
                NotificationType = NotificationType.NotFound,
                Message = SubcategoryConstants.SUBCATEGORY_DOESNT_EXIST,
            };

        return new ApiResponse<SubcategoryDetailsDTO>
        {
            NotificationType = NotificationType.Success,
            Data = new SubcategoryDetailsDTO()
            {
                Id = subcategory.Id,
                Name = subcategory.Name,
                CategoryId = subcategory.Category.Id,
                Category = subcategory.Category.Name
            }
        };
    }



    private bool HasRelatedEntities(Subcategory subcategory)
    {
        return subcategory.Products?.Any(x => !x.IsDeleted) == true;
    }
}
