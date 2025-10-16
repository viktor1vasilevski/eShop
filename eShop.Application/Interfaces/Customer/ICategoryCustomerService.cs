using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;

namespace eShop.Application.Interfaces.Customer;

public interface ICategoryCustomerService
{
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default);
}


public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubcategoryCount { get; set; }
    public int ProductCount { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = [];
}