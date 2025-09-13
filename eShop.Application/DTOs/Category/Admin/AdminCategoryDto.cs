namespace eShop.Application.DTOs.Category.Admin;

public class AdminCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
