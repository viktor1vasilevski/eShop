using eShop.Application.DTOs.Order;
using eShop.Application.DTOs.OrderItem;
using eShop.Application.Enums;
using eShop.Application.Helpers.Admin;
using eShop.Application.Interfaces.Admin;
using eShop.Application.Requests.Admin.Order;
using eShop.Application.Responses;
using eShop.Application.Responses.Admin.Order;
using eShop.Application.Responses.Admin.OrderItem;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services.Admin;

public class OrderAdminService(IUnitOfWork _uow) : IOrderAdminService
{
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>();

    public ApiResponse<List<OrderDetailsAdminDto>> GetOrders(OrderAdminRequest request)
    {
        var query = _orderRepository.GetAsQueryable(
            orderBy: x => x.OrderByDescending(x => x.Created),
            include: x => x.Include(x => x.OrderItems).ThenInclude(p => p.Product));

        var ordersDTO = query.Select(order => new OrderDetailsAdminDto
        {
            TotalAmount = order.TotalAmount,
            OrderCreatedOn = order.Created,
            Items = order.OrderItems.Select(item => new OrderItemAdminDto
            {
                ProductName = item.Product!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Image = ImageDataUriBuilder.FromImage(item.Product.Image)
            }).ToList()
        }).ToList();

        return new ApiResponse<List<OrderDetailsAdminDto>>
        {
            Data = ordersDTO,
            Status = ResponseStatus.Success,
        };
    }
}
