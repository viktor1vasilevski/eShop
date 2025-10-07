using eShop.Application.Interfaces.Shared;
using eShop.Application.Requests.Shared.Auth;
using eShop.Application.Responses.Shared.Auth;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace eShop.Application.Services.Admin;

public class AdminAuthService(IUnitOfWork _uow, IConfiguration _configuration, IPasswordHasher _passwordHasher) : IAuthService
{
    private readonly IRepository<User> _userRepository = _uow.GetRepository<User>();

    public async Task<ApiResponse<LoginDto>> LoginAsync(UserLoginRequest request)
    {

        // Build query using QueryAsync
        var (products, totalCount) = await _userRepository.QueryAsync(q =>
            q.Where(p => !p.IsDeleted)
             .WhereIf(!string.IsNullOrEmpty(request.Name), p => p.Name.ToLower().Contains(request.Name.ToLower()))
             .Include(p => p.Category)
             .OrderBy(p => request.SortDirection == "asc"
                            ? p.Name
                            : p.Name)
        );
    }
}
