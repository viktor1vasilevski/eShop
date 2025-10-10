using eShop.Application.Interfaces.Customer;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class BasketCustomerService(IUnitOfWork _uow) : IBasketCustomerService
{
    private readonly IEfRepository<Basket> _basketRepository = _uow.GetEfRepository<Basket>();
}
