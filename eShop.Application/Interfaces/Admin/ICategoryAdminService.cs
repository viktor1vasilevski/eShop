using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;

namespace eShop.Application.Interfaces.Admin;

public interface ICategoryAdminService
{
    // queries
    Task<ApiResponse<List<CategoryAdminDto>>> GetCategoriesAsync(CategoryAdminRequest request);
    Task<ApiResponse<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id);
    Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();

    // commands
    Task<ApiResponse<CategoryAdminDto>> CreateCategoryAsync(CreateCategoryRequest request);
    Task<ApiResponse<CategoryAdminDto>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    Task<ApiResponse<CategoryAdminDto>> DeleteCategoryAsync(Guid id);
}
