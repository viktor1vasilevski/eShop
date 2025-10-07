using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Admin;

public interface ICategoryAdminService
{
    Task<ApiResponse<List<CategoryAdminDto>>> GetCategories(CategoryAdminRequest request);
    Task<ApiResponse<CategoryAdminDto>> CreateCategory(CreateCategoryRequest request);
}
