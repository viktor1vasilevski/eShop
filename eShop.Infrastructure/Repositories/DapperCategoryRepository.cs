using Dapper;
using eShop.Domain.Interfaces;
using System.Data;

namespace eShop.Infrastructure.Repositories
{
    public class DapperCategoryRepository : IDapperCategoryRepository
    {
        private readonly IDbConnection _dbConnection;

        public DapperCategoryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<CategoryTreeDto>> GetCategoryTreeForMenuAsync()
        {
            var sql = @"
                SELECT Id, Name, ParentCategoryId
                FROM Categories
                WHERE IsDeleted = 0
            ";

            var categories = (await _dbConnection.QueryAsync<CategoryTreeDto>(sql)).ToList();

            return BuildCustomerCategoryTree(categories);
        }

        private List<CategoryTreeDto> BuildCustomerCategoryTree(List<CategoryTreeDto> categories, Guid? parentId = null)
        {
            return categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c =>
                {
                    var children = BuildCustomerCategoryTree(categories, c.Id);

                    bool isLeaf = !children.Any();

                    return new CategoryTreeDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Children = children
                    };
                })
                .ToList();
        }
    }
}
