namespace eShop.Application.DTOs.Customer.Category;

public class CategoryNode
{
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }
}
