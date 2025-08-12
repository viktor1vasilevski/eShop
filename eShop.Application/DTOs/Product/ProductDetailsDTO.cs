namespace eShop.Application.DTOs.Product;

public class ProductDetailsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public bool CanComment { get; set; }
    public Guid SubcategoryId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
