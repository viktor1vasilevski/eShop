using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Subcategory;

public class SubcategoryRefDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<ProductRefDTO>? Products { get; set; }
}
