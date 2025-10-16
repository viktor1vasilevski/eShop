using eShop.Application.Interfaces.Customer;
using eShop.Domain.Interfaces;

namespace eShop.Application.Responses.Admin.Category;

public class CategoryEditAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? Image { get; set; }
    public List<CategoryTreeDto> ValidParentTree { get; set; } = [];
}
