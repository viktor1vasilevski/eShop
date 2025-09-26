using eShop.Application.Requests.Admin;

namespace eShop.Application.Requests.Comment;

public class CommentRequest : BaseAdminRequest
{
    public Guid ProductId { get; set; }
}
