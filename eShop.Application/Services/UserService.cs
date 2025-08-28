using eShop.Application.DTOs.User;
using eShop.Application.Enums;
using eShop.Application.Extensions;
using eShop.Application.Interfaces;
using eShop.Application.Requests.User;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Enums;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services;

public class UserService(IUnitOfWork _uow) : IUserService
{
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();

    public ApiResponse<UserDetailsDTO> GetUserDetailsById(Guid userId)
    {
        var query = _userRepository.GetAsQueryable(
            filter: x => x.Id == userId,
            include: x => x.Include(x => x.Orders)
            );

        return new ApiResponse<UserDetailsDTO>
        {

        };
    }

    public ApiResponse<List<UserDTO>> GetUsers(UserRequest request)
    {
        var query = _userRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(!String.IsNullOrEmpty(request.FirstName), x => x.FirstName.ToLower().Contains(request.FirstName.ToLower()))
                          .WhereIf(!String.IsNullOrEmpty(request.LastName), x => x.LastName.ToLower().Contains(request.LastName.ToLower()))
                          .WhereIf(!String.IsNullOrEmpty(request.Username), x => x.Username.ToLower().Contains(request.Username.ToLower()))
                          .WhereIf(!String.IsNullOrEmpty(request.Email), x => x.Email.ToLower().Contains(request.Email.ToLower()))
                    .Where(x => x.Role == Role.Customer));

        var totalCount = query.Count();

        var sortedQuery = query;
        if (!string.IsNullOrEmpty(request.SortBy) && !string.IsNullOrEmpty(request.SortDirection))
        {
            if (request.SortDirection.ToLower() == "asc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderBy(x => x.Created),
                    _ => sortedQuery.OrderBy(x => x.Created)
                };
            }
            else if (request.SortDirection.ToLower() == "desc")
            {
                sortedQuery = request.SortBy.ToLower() switch
                {
                    "created" => sortedQuery.OrderByDescending(x => x.Created),
                    _ => sortedQuery.OrderByDescending(x => x.Created)
                };
            }
        }

        if (request.Skip.HasValue)
            sortedQuery = sortedQuery.Skip(request.Skip.Value);

        if (request.Take.HasValue)
            sortedQuery = sortedQuery.Take(request.Take.Value);

        var usersDTO = sortedQuery.Select(x => new UserDTO
        {
            Id = x.Id,
            FirstName = x.FirstName,
            LastName = x.LastName,
            Email = x.Email,
            Username = x.Username,
            Created = x.Created,
        }).ToList();

        return new ApiResponse<List<UserDTO>>
        {
            Data = usersDTO,
            Status = ResponseStatus.Success,
            TotalCount = totalCount,
        };
    }
}
