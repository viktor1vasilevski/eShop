using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Basket;
using eShop.Application.Responses.Customer.Basket;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class BasketCustomerService(IUnitOfWork _uow) : IBasketCustomerService
{
    private readonly IEfRepository<Basket> _basketRepository = _uow.GetEfRepository<Basket>();
    private readonly IEfRepository<Product> _productRepository = _uow.GetEfRepository<Product>();
    private readonly IEfRepository<User> _userRepository = _uow.GetEfRepository<User>();

    public async Task<ApiResponse<BasketCustomerDto>> ClearBasketItemsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<BasketCustomerDto>> GetBasketByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<BasketCustomerDto>> RemoveItemAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<BasketCustomerDto>> UpdateUserBasketAsync(Guid userId, UpdateBasketCustomerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
