namespace eShop.Application.Requests.Admin.Product;

public class GenerateAIProductDescriptionRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string Categories { get; set; } = string.Empty;
}
