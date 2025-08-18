namespace eShop.Application.Requests.Comment;

public class CommentRequest : BaseRequest
{
    public Guid ProductId { get; set; }
}
