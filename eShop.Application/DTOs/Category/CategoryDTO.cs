namespace eShop.Application.DTOs.Category;

public class CategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
