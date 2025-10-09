namespace eShop.Application.Requests.Admin.Product;

public class ProductAdminRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitQuantity { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
