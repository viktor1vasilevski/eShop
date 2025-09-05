using eShop.Application.DTOs.Subcategory;

namespace eShop.Application.DTOs.Category;

public class CategoryDetailsDTO : CategoryDTO
{
    public string Image { get; set; } = string.Empty;
    public List<SubcategoryRefDTO>? Subcategories { get; set; }
}
