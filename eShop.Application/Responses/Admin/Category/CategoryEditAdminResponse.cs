using eShop.Application.Interfaces.Customer;

namespace eShop.Application.Responses.Admin.Category;

public class CategoryEditAdminResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Image { get; set; }
    public List<CategoryTreeResponse> ValidParentTree { get; set; } = [];
}
