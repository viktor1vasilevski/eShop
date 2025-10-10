namespace eShop.Application.Requests.Customer.Comment;

public class CreateCommentCustomerRequest
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public int Rating { get; set; }
}
