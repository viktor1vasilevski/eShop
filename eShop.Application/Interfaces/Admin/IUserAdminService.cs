using eShop.Application.Requests.Admin.User;
using eShop.Application.Responses;
using eShop.Application.Responses.Admin.User;

namespace eShop.Application.Interfaces.Admin;

public interface IUserAdminService
{
    ApiResponse<List<UserDto>> GetUsers(UserRequest request);
}
