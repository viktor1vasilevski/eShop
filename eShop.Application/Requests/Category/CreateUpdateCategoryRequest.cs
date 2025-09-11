namespace eShop.Application.Requests.Category;

public class CreateUpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string Image { get; set; } = null!;
}

