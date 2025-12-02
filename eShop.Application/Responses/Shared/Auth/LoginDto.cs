using eShop.Domain.Enums;

namespace eShop.Application.Responses.Shared.Auth;

public class LoginDto
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}
