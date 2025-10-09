using eShop.Application.DTOs.Admin.Category;
using eShop.Application.DTOs.Admin.Product;

namespace eShop.Application.Responses.Admin.Category;

public class CategoryDetailsAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<ProductRefDto> Products { get; set; } = [];
    public List<CategoryRefDto> Children { get; set; } = [];
}
