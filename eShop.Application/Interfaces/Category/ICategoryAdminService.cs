using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.Requests.Category;

namespace eShop.Application.Interfaces.Category;

public interface ICategoryAdminService
{
    ApiResponse<List<AdminCategoryDto>> GetCategories(CategoryRequest request);
    ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request);
    Task<ApiResponse<CategoryDto>> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<AdminCategoryDto> DeleteCategory(Guid id);
    Task<ApiResponse<AdminCategoryDetailsDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<CategoryEditDto>> GetCategoryForEditAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();
}
