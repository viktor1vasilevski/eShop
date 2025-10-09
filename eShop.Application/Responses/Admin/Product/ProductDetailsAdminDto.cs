using eShop.Application.DTOs.Admin.Category;

namespace eShop.Application.Responses.Admin.Product;

public class ProductDetailsAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitQuantity { get; set; }
    public string Image { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public List<CategoryRefDto> Categories { get; set; } = [];
}
