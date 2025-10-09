namespace eShop.Application.Requests.Admin.Product;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; } = null!;
}
