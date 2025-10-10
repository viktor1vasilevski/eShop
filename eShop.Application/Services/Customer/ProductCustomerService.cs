using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class ProductCustomerService(IUnitOfWork _uow) : IProductCustomerService
{
    private readonly IEfRepository<Product> _productService = _uow.GetEfRepository<Product>();


    public async Task<ApiResponse<List<ProductCustomerDto>>> GetProductsAsync(ProductCustomerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
