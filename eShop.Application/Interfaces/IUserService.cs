using eShop.Application.DTOs.User;
using eShop.Application.Requests.User;
using eShop.Application.Responses;

namespace eShop.Application.Interfaces;

public interface IUserService
{
    ApiResponse<List<UserDTO>> GetUsers(UserRequest request);
}
