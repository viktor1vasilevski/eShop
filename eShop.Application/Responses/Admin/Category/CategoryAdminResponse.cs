namespace eShop.Application.Responses.Admin.Category;

public class CategoryAdminResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
