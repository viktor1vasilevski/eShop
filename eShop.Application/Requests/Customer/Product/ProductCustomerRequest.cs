namespace eShop.Application.Requests.Customer.Product;

public class ProductCustomerRequest
{
    public Guid CategoryId { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
