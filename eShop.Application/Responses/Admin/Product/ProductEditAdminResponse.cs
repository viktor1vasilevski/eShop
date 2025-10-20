namespace eShop.Application.Responses.Admin.Product;

public class ProductEditAdminResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitQuantity { get; set; }
    public string Image { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
}
