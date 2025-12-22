using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.Dapper;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CustomerCategoryService(IDapperRepository<Category> _categoryDapperRepository) : ICustomerCategoryService
{
    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default)
    {
        const string categoriesSql = @"
            SELECT Id, Name, ParentCategoryId
            FROM Categories
            WHERE IsDeleted = 0
            ORDER BY Name;";

        var allCategories = (await _categoryDapperRepository
            .QueryAsync<CategoryTreeDto>(categoriesSql, cancellationToken: cancellationToken))
            .ToList();

        const string productCategoriesSql = @"
            SELECT DISTINCT CategoryId
            FROM Products
            WHERE IsDeleted = 0;";

        var categoriesWithProducts = (await _categoryDapperRepository
            .QueryAsync<Guid>(productCategoriesSql, cancellationToken: cancellationToken))
            .ToHashSet();

        var tree = BuildCategoryTree(allCategories, categoriesWithProducts);

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

    private static List<CategoryTreeDto> BuildCategoryTree(
    List<CategoryTreeDto> allCategories,
    HashSet<Guid> categoriesWithProducts,
    Guid? parentId = null)
    {
        var result = new List<CategoryTreeDto>();

        foreach (var category in allCategories.Where(c => c.ParentCategoryId == parentId))
        {
            var children = BuildCategoryTree(allCategories, categoriesWithProducts, category.Id);

            if (categoriesWithProducts.Contains(category.Id) || children.Any())
            {
                category.Children = children;
                result.Add(category);
            }
        }

        return result;
    }
}

