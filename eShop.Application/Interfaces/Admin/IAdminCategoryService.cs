using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;

namespace eShop.Application.Interfaces.Admin;

public interface IAdminCategoryService
{
    Task<ApiResponse<List<CategoryAdminDto>>> GetCategoriesAsync(CategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminDto>> CreateCategoryAsync(CreateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminDto>> UpdateCategoryAsync(Guid id, UpdateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminDto>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

