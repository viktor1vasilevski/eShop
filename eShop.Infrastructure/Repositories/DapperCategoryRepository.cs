using Dapper;
using eShop.Domain.Interfaces;
using System.Data;

namespace eShop.Infrastructure.Repositories;

public class DapperCategoryRepository(IDbConnection _dbConnection) : IDapperCategoryRepository
{


    public async Task<List<CategoryTreeDto>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken)
    {
        const string categoriesSql = @"
            SELECT Id, Name, ParentCategoryId
            FROM Categories
            WHERE IsDeleted = 0
            ORDER BY Name;";

        var categoriesCommand = new CommandDefinition(categoriesSql, cancellationToken: cancellationToken);
        var allCategories = (await _dbConnection.QueryAsync<CategoryTreeDto>(categoriesCommand)).ToList();

        const string productCategoriesSql = @"
            SELECT DISTINCT CategoryId
            FROM Products
            WHERE IsDeleted = 0;";

        var productCategoriesCommand = new CommandDefinition(productCategoriesSql, cancellationToken: cancellationToken);
        var categoriesWithProducts = (await _dbConnection.QueryAsync<Guid>(productCategoriesCommand)).ToHashSet();

        return BuildCategoryTreeWithProducts(allCategories, categoriesWithProducts);
    }

    private List<CategoryTreeDto> BuildCategoryTreeWithProducts(
        List<CategoryTreeDto> allCategories,
        HashSet<Guid> categoriesWithProducts,
        Guid? parentId = null)
    {
        var result = new List<CategoryTreeDto>();

        foreach (var category in allCategories.Where(c => c.ParentCategoryId == parentId))
        {
            var children = BuildCategoryTreeWithProducts(allCategories, categoriesWithProducts, category.Id);

            if (categoriesWithProducts.Contains(category.Id) || children.Any())
            {
                category.Children = children;
                result.Add(category);
            }
        }

        return result;
    }



}
