using eShop.Application.Requests.Customer.Product;
using eShop.Application.Responses.Customer.Product;

namespace eShop.Application.Interfaces.Customer;

public interface IProductCustomerService
{
    ApiResponse<List<ProductCustomerDto>> GetProducts(ProductCustomerRequest request);
    ApiResponse<ProductDetailsCustomerDto> GetProductById(Guid id);
}
