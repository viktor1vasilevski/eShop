namespace eShop.Application.DTOs.Admin.Category;

public class CategoryFlatDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public int ProductCount { get; set; }
}
