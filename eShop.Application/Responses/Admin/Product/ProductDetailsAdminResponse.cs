using eShop.Application.DTOs.Admin.Category;
using eShop.Domain.Enums;

namespace eShop.Application.Responses.Admin.Product;

public class ProductDetailsAdminResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int UnitQuantity { get; set; }
    public string Image { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public List<CategoryRefDto> Categories { get; set; } = [];
    public List<CommentDto> Comments { get; set;  } = [];
}

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string CommentText { get; set; } = null!;
    public Rating Rating { get; set; }
    public DateTime Created { get; set; }
}

