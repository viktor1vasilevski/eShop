using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;
using eShop.Application.Responses.Shared.Base;

namespace eShop.Application.Interfaces.Customer;

public interface IProductCustomerService
{
    Task<ApiResponse<List<ProductCustomerResponse>>> GetProductsAsync(ProductCustomerRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailsCustomerResponse>> GetProductByIdAsync(Guid productId, Guid? userId = null, CancellationToken cancellationToken = default);

}
