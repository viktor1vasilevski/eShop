namespace eShop.Application.Requests.Admin.Product;

public class ProductAdminRequest : BaseRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitQuantity { get; set; }
}
