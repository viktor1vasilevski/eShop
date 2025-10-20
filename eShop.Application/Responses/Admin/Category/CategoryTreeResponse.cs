namespace eShop.Application.Responses.Admin.Category;

public class CategoryTreeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubcategoryCount { get; set; }
    public int ProductCount { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryTreeResponse> Children { get; set; } = [];
}
