using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IProductDescriptionGenerator
{
    /// <summary>
    /// Generates a product description based on product details.
    /// </summary>
    /// <param name="productName">The name of the product.</param>
    /// <param name="category">The category the product belongs to.</param>
    /// <param name="subcategory">The subcategory the product belongs to.</param>
    /// <param name="additionalContext">Optional details like features, style, or target audience.</param>
    /// <returns>The generated description text.</returns>
    Task<ApiResponse<string>> GenerateDescriptionAsync(string productName, string category, string subcategory, string? additionalContext = null);
}
