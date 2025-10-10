using eShop.Application.Interfaces.Customer;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;

namespace eShop.Application.Services.Customer;

public class OrderCustomerService(IUnitOfWork _uow) : IOrderCustomerService
{
    private readonly IEfRepository<Order> _orderRepository = _uow.GetEfRepository<Order>();
}
