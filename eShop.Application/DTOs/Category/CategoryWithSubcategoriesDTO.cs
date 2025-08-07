using eShop.Application.DTOs.Subcategory;

namespace eShop.Application.DTOs.Category;

public class CategoryWithSubcategoriesDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<SelectSubcategoryListItemDTO> Subcategories { get; set; }
}
