using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface ICustomerProductService
{
    Task<Result<List<ProductCustomerDto>>> GetProductsAsync(ProductCustomerRequest request, CancellationToken cancellationToken = default);
    Task<Result<ProductDetailsCustomerDto>> GetProductByIdAsync(Guid productId, Guid? userId = null, CancellationToken cancellationToken = default);

}
