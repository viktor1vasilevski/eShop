using eShop.Application.DTOs.Category;

namespace eShop.Application.Interfaces.Customer;

public interface ICategoryCustomerService
{
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync();
}
