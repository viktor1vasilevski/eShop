using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.Requests.Category;

namespace eShop.Application.Interfaces.Category;

public interface ICategoryAdminService
{
    ApiResponse<List<AdminCategoryDto>> GetCategories(CategoryRequest request);
    ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDto> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<CategoryDetailsDto> DeleteCategory(Guid id);
    Task<ApiResponse<CategoryDetailsDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();
}
