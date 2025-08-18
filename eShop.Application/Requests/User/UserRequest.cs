namespace eShop.Application.Requests.User;

public class UserRequest : BaseRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
}
