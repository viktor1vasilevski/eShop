using eShop.Domain.Enums;

namespace eShop.Application.DTOs.Auth;

public class LoginDTO
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}
