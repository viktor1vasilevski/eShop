using eShop.Application.DTOs.Category;

namespace eShop.Application.Interfaces.Category;

public interface ICategoryCustomerService
{
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync();
}
