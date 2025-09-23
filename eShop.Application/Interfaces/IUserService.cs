using eShop.Application.DTOs.User;
using eShop.Application.Requests.User;


namespace eShop.Application.Interfaces;

public interface IUserService
{
    ApiResponse<List<UserDTO>> GetUsers(UserRequest request);
    ApiResponse<UserDetailsDTO> GetUserDetailsById(Guid userId);
}
