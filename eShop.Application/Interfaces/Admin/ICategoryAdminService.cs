using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;

namespace eShop.Application.Interfaces.Admin;

public interface ICategoryAdminService
{
    // queries
    ApiResponse<List<CategoryAdminDto>> GetCategories(CategoryAdminRequest request);
    Task<ApiResponse<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();


    // commands
    ApiResponse<CategoryAdminDto> CreateCategory(CreateCategoryRequest request);
    Task<ApiResponse<CategoryAdminDto>> UpdateCategory(Guid id, UpdateCategoryRequest request);
    ApiResponse<CategoryAdminDto> DeleteCategory(Guid id);

}
