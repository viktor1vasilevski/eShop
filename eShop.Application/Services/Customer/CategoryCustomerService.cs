using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace eShop.Application.Services.Customer;

public class CategoryCustomerService(IUnitOfWork _uow) : ICategoryCustomerService
{
    private readonly IEfRepository<Category> _categorySerice = _uow.GetEfRepository<Category>();


    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default)
    {
        var (categories, _) = await _categorySerice.QueryAsync(
                queryBuilder: q => q.Where(x => !x.IsDeleted),
                selector: c => c,
                includeBuilder: x => x.Include(x => x.Products)
                                      .Include(x => x.Children),
                cancellationToken: cancellationToken);

        var tree = BuildCustomerCategoryTree(categories.ToList());

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

    private List<CategoryTreeDto> BuildCustomerCategoryTree(List<Category> categories, Guid? parentId = null)
    {
        return categories
            .Where(c => c.ParentCategoryId == parentId)
            .Select(c =>
            {
                var children = BuildCustomerCategoryTree(categories, c.Id);

                bool isLeaf = !children.Any();
                bool hasProducts = c.Products != null && c.Products.Any();

                if (isLeaf && !hasProducts)
                    return null;

                if (!isLeaf && !children.Any())
                    return null;

                return new CategoryTreeDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Children = children
                };
            })
            .Where(x => x != null)
            .ToList()!;
    }
}
