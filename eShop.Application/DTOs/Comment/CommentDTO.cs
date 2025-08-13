namespace eShop.Application.DTOs.Comment;

public class CommentDTO
{
    public string? CommentText { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public int Rating { get; set; }
}
