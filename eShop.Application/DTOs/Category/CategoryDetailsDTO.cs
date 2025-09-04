using eShop.Application.DTOs.Subcategory;

namespace eShop.Application.DTOs.Category;

public class CategoryDetailsDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public List<SubcategoryDTO>? Subcategories { get; set; }
}
