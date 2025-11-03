using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICategoryCustomerService
{
    Task<ApiResponse<List<CategoryTreeResponse>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default);
}