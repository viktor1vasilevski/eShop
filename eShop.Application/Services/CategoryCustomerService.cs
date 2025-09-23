using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Admin.Category;

namespace eShop.Application.Services;

public class CategoryCustomerService(IUnitOfWork _uow) : ICategoryCustomerService
{
    private readonly IRepositoryBase<Category> _categoryRepository = _uow.GetRepository<Category>();


    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync()
    {
        var allCategories = (await _categoryRepository.GetAsync(
                filter: x => !x.IsDeleted,
                include: q => q
                    .Include(c => c.Products)
                    .Include(c => c.Children)
            )).ToList();

        var tree = BuildCustomerCategoryTree(allCategories);

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

                // Filter logic for customers
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
