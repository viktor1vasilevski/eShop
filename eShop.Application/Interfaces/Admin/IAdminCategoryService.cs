using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Admin.Category;
using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;

namespace eShop.Application.Interfaces.Admin;

public interface IAdminCategoryService
{
    Task<Result<List<CategoryAdminDto>>> GetCategoriesAsync(CategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryDetailsAdminDto>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<CategoryEditAdminDto>> GetCategoryForEditAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<List<CategoryTreeDto>>> GetCategoryTreeAsync(CancellationToken cancellationToken = default);
    Task<Result<CategoryAdminDto>> CreateCategoryAsync(CreateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryAdminDto>> UpdateCategoryAsync(Guid id, UpdateCategoryAdminRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryAdminDto>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

