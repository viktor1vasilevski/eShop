using eShop.Application.Responses.Admin.Category;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerCategoryService
{
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default);
}