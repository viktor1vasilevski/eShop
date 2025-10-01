namespace eShop.Application.Responses.Customer.Comment;

public class CommentCustomerDto
{
    public string CommentText { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public int Rating { get; set; }
}
