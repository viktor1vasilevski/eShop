using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Subcategory;

public class SubcategoryRefDTO : BaseDTO
{
    public List<ProductRefDTO>? Products { get; set; }
}
