namespace eShop.Application.DTOs.Category.Admin;

public class CategoryEditDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Image { get; set; }


    public List<CategoryRefDto> Children { get; set; } = new();
}
