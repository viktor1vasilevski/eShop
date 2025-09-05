using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Subcategory;

public class SubcategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
