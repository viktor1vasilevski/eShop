namespace eShop.Application.Responses.Customer.Product;

public class ProductCustomerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Image { get; set; } = null!;
    public string Category { get; set; } = string.Empty;

}
