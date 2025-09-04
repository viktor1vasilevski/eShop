using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Subcategory;

public class SubcategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public DateTime? LastModified { get; set; }
    public List<ProductDTO>? Products { get; set; }
}
