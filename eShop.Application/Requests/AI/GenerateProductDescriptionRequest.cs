namespace eShop.Application.Requests.AI;

public class GenerateProductDescriptionRequest
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string? AdditionalContext { get; set; }
}
