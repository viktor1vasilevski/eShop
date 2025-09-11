using eShop.Application.DTOs.Product;

namespace eShop.Application.DTOs.Category;

public class CategoryDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public List<ProductRefDto> Products { get; set; } = new();
    public List<CategoryRefDto> Children { get; set; } = new();
}
