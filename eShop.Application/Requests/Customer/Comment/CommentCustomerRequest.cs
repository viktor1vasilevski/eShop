namespace eShop.Application.Requests.Customer.Comment;

public class CommentCustomerRequest
{
    public Guid ProductId { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
}
