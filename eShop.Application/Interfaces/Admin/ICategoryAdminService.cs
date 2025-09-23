using eShop.Application.DTOs.Category;
using eShop.Application.DTOs.Category.Admin;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Requests.Category;
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
    ApiResponse<CategoryAdminDto> CreateCategory(CreateUpdateCategoryRequest request);
    Task<ApiResponse<CategoryAdminDto>> UpdateCategory(Guid id, CreateUpdateCategoryRequest request);
    ApiResponse<CategoryAdminDto> DeleteCategory(Guid id);

}
