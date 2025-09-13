namespace eShop.Application.Requests.Product;

public class ProductRequest : BaseRequest
{
    public string? Name { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? UnitQuantity { get; set; }

}
