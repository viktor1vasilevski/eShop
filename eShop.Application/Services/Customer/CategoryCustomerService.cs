using eShop.Application.Interfaces.Customer;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class CategoryCustomerService(IUnitOfWork _uow) : ICategoryCustomerService
{
    private readonly IEfRepository<Category> _categoryCustomerSerice = _uow.GetEfRepository<Category>();


    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
