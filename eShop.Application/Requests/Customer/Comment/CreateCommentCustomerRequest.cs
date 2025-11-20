using eShop.Domain.Enums;

namespace eShop.Application.Requests.Customer.Comment;

public class CreateCommentCustomerRequest
{
    public Guid ProductId { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public Rating Rating { get; set; }
}
