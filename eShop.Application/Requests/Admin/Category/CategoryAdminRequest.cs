namespace eShop.Application.Requests.Admin.Category;

public class CategoryAdminRequest
{
    public string? Name { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
