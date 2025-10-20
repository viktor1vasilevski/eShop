using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;

namespace eShop.Application.Interfaces.Admin;

public interface ICategoryAdminService
{
    Task<ApiResponse<List<CategoryAdminResponse>>> GetCategoriesAsync(CategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryDetailsAdminResponse>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryEditAdminResponse>> GetCategoryForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<CategoryTreeResponse>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminResponse>> CreateCategoryAsync(CreateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CategoryAdminResponse>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

