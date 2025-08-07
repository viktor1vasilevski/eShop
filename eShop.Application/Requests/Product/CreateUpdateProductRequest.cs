namespace eShop.Application.Requests.Product;

public class CreateUpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid SubcategoryId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
}
