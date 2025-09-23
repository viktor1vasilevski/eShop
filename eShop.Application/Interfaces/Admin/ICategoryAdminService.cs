using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.Requests.Category;
using eShop.Application.Responses.Admin.Category;

namespace eShop.Application.Interfaces.Admin;

public interface ICategoryAdminService
{
    // queries
    ApiResponse<List<CategoryAdminResponse>> GetCategories(CategoryRequest request);
    Task<ApiResponse<CategoryDetailsAdminResponse>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<CategoryEditDto>> GetCategoryForEditAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();


    // commands
    ApiResponse<CategoryDto> CreateCategory(CreateUpdateCategoryRequest request);
    Task<ApiResponse<CategoryDto>> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<AdminCategoryDto> DeleteCategory(Guid id);

}
