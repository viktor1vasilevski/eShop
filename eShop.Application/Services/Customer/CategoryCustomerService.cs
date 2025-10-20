using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.Dapper;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CategoryCustomerService(IDapperRepository<Category> _categoryDapperRepository) : ICategoryCustomerService
{
    public async Task<ApiResponse<List<CategoryTreeResponse>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default)
    {
        const string categoriesSql = @"
            SELECT Id, Name, ParentCategoryId
            FROM Categories
            WHERE IsDeleted = 0
            ORDER BY Name;";

        var allCategories = (await _categoryDapperRepository
            .QueryAsync<CategoryTreeResponse>(categoriesSql, cancellationToken: cancellationToken))
            .ToList();

        const string productCategoriesSql = @"
            SELECT DISTINCT CategoryId
            FROM Products
            WHERE IsDeleted = 0;";

        var categoriesWithProducts = (await _categoryDapperRepository
            .QueryAsync<Guid>(productCategoriesSql, cancellationToken: cancellationToken))
            .ToHashSet();

        var tree = BuildCategoryTree(allCategories, categoriesWithProducts);

        return new ApiResponse<List<CategoryTreeResponse>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

    private static List<CategoryTreeResponse> BuildCategoryTree(
    List<CategoryTreeResponse> allCategories,
    HashSet<Guid> categoriesWithProducts,
    Guid? parentId = null)
    {
        var result = new List<CategoryTreeResponse>();

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

