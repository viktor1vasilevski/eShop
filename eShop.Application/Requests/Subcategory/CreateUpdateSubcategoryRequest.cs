namespace eShop.Application.Requests.Subcategory;

public class CreateUpdateSubcategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Image { get; set; } = string.Empty;
}
