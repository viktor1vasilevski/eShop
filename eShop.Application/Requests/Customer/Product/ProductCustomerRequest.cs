namespace eShop.Application.Requests.Customer.Product;

public class ProductCustomerRequest : BaseCustomerRequest
{
    public Guid CategoryId { get; set; }
}
