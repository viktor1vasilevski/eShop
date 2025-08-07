using eShop.Application.DTOs.Category;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Category;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;

namespace eShop.Application.Services;

public class CategoryService(IUnitOfWork _uow) : ICategoryService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();


    public ApiResponse<List<CategoryDTO>> GetCategories(CategoryRequest request)
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

        var categoriesDTO = categories.Select(x => new CategoryDTO
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
            NotificationType = NotificationType.Success,
        };
    }
}
