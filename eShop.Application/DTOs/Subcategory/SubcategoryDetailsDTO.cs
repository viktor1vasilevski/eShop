using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Subcategory;

public class SubcategoryDetailsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = String.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public string Category { get; set; } = String.Empty;
    public Guid CategoryId { get; set; }
    public List<ProductRefDTO>? Products { get; set; }
}
