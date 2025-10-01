namespace eShop.Application.Requests.Customer.Comment;

public class CommentRequest
{
    public Guid ProductId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
