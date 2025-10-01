namespace eShop.Application.Requests.Admin.AI;

public class GenerateProductDescriptionRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string Categories { get; set; } = string.Empty;
}
