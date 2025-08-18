using eShop.Application.DTOs.User;
using eShop.Application.Enums;
using eShop.Application.Interfaces;
using eShop.Application.Requests.User;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;

namespace eShop.Application.Services;

public class UserService(IUnitOfWork _uow) : IUserService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();


    public ApiResponse<List<UserDTO>> GetUsers(UserRequest request)
    {
        var query = _userRepository.GetAsQueryableWhereIf();

        var totalCount = query.Count();

        var usersDTO = query.Select(x => new UserDTO
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
        }).ToList();

        return new ApiResponse<List<UserDTO>>
        {
            Data = usersDTO,
            NotificationType = NotificationType.Success,
            TotalCount = totalCount,
        };
    }
}
