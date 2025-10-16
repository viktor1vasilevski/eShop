using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.EntityFramework;

namespace eShop.Application.Services.Customer;

public class CategoryCustomerService(IUnitOfWork _uow, IDapperCategoryRepository _dapperCategoryRepository) : ICategoryCustomerService
{

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default)
    {
        var tree = await _dapperCategoryRepository.GetCategoryTreeForMenuAsync(cancellationToken);

        return new ApiResponse<List<CategoryTreeDto>>
        {
            Data = tree,
            Status = ResponseStatus.Success
        };
    }

}
