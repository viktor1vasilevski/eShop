namespace eShop.Application.DTOs.Category;

public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<CategoryTreeDto> Children { get; set; } = new();
}

