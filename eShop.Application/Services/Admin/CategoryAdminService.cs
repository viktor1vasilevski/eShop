using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Exceptions;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using eShop.Domain.ValueObject;
using Microsoft.Extensions.Logging;

namespace eShop.Application.Services.Admin;

public class CategoryAdminService(IUnitOfWork _uow, ILogger<CategoryAdminService> _logger) : ICategoryAdminService
{
    private readonly IEfRepository<Category> _categoryAdminService = _uow.GetEfRepository<Category>();


    public async Task<ApiResponse<List<CategoryAdminDto>>> GetCategories(CategoryAdminRequest request)
    {
        string? sortBy = request.SortBy?.Trim().ToLower();
        string? sortDirection = request.SortDirection?.Trim().ToLower();

        Func<IQueryable<Category>, IOrderedQueryable<Category>>? orderBy = null;
        if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(sortDirection))
        {
            orderBy = sortBy switch
            {
                "created" => sortDirection == "asc"
                                ? q => q.OrderBy(x => x.Created)
                                : q => q.OrderByDescending(x => x.Created),
                "lastmodified" => sortDirection == "asc"
                                ? q => q.OrderBy(x => x.LastModified)
                                : q => q.OrderByDescending(x => x.LastModified),
                _ => sortDirection == "asc"
                        ? q => q.OrderBy(x => x.Created)
                        : q => q.OrderByDescending(x => x.Created)
            };
        }

        var (categories, totalCount) = await _categoryAdminService.QueryAsync(
            queryBuilder: q => q.WhereIf(true, x => !x.IsDeleted)
                                .WhereIf(!string.IsNullOrEmpty(request.Name), x => x.Name.ToLower().Contains(request.Name.ToLower())),
            selector: c => new CategoryAdminDto
            {
                Id = c.Id,
                Name = c.Name,
                Created = c.Created,
                LastModified = c.LastModified
            },
            orderBy: orderBy,
            skip: request.Skip,
            take: request.Take
        );

        return new ApiResponse<List<CategoryAdminDto>>
        {
            Data = categories.ToList(),
            TotalCount = totalCount,
            Status = ResponseStatus.Success
        };
    }



    public async Task<ApiResponse<CategoryAdminDto>> CreateCategory(CreateCategoryRequest request)
    {
        try
        {
            var trimmedName = request.Name.Trim();
            var normalizedName = trimmedName.ToLower();

            if (await _categoryAdminService.ExistsAsync(x =>
                x.Name.ToLower() == normalizedName &&
                x.ParentCategoryId == request.ParentCategoryId &&
                !x.IsDeleted))
                return new ApiResponse<CategoryAdminDto>
                {
                    Status = ResponseStatus.Conflict,
                    Message = "Category Exist"
                };

            var (bytes, type) = ImageParsing.FromBase64(request.Image);
            var image = Image.FromBytes(bytes, type);

            var category = Category.Create(trimmedName, image, request.ParentCategoryId);
            await _categoryAdminService.AddAsync(category);
            _uow.SaveChanges();

            return new ApiResponse<CategoryAdminDto>
            {
                Status = ResponseStatus.Created,
                Message = "Category created",
                Data = new CategoryAdminDto
                {
                    Id = category.Id,
                    Name = category.Name,
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

}
