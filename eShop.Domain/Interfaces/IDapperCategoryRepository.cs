namespace eShop.Domain.Interfaces;

public interface IDapperCategoryRepository
{
    Task<List<CategoryTreeDto>> GetCategoryTreeForMenuAsync(CancellationToken cancellationToken = default);
}

public class CategoryTreeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubcategoryCount { get; set; }
    public int ProductCount { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public List<CategoryTreeDto> Children { get; set; } = [];
}
