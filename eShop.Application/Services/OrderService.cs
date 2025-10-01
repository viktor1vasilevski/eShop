using eShop.Application.DTOs.Order;
using eShop.Application.DTOs.OrderItem;
using eShop.Application.Enums;
using eShop.Application.Helpers.Admin;
using eShop.Application.Interfaces;
using eShop.Application.Requests.Order;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eShop.Application.Services;

public class OrderService(IUnitOfWork _uow) : IOrderService
{
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>(); 

    public ApiResponse<List<OrderDetailsDTO>> GetOrders(OrderRequest request)
    {
        var query = _orderRepository.GetAsQueryable(
            orderBy: x => x.OrderByDescending(x => x.Created),
            include: x => x.Include(x => x.OrderItems).ThenInclude(p => p.Product));

        var ordersDTO = query.Select(order => new OrderDetailsDTO
        {
            TotalAmount = order.TotalAmount,
            OrderCreatedOn = order.Created,
            Items = order.OrderItems.Select(item => new OrderItemDTO
            {
                ProductName = item.Product!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Image = ImageDataUriBuilder.FromImage(item.Product.Image)
            }).ToList()
        }).ToList();
       
        return new ApiResponse<List<OrderDetailsDTO>>
        {
            Data = ordersDTO,
            Status = ResponseStatus.Success,
        };
    }

    public ApiResponse<List<OrderDetailsDTO>> GetOrdersForUserId(Guid userId)
    {
        var response = new ApiResponse<List<OrderDetailsDTO>>();

        var query = _orderRepository.GetAsQueryable(
            filter: x => x.UserId == userId,
            orderBy: x => x.OrderByDescending(x => x.Created),
            include: x => x.Include(x => x.OrderItems).ThenInclude(p => p.Product)).AsNoTracking();

        var totalCount = query.Count();

        if (query is null || query.Count() == 0)
        {
            response.Status = ResponseStatus.Success;
            response.Data = [];
            return response;
        }

        var ordersDto = query.Select(order => new OrderDetailsDTO
        {
            FirstName = order.User.FirstName,
            LastName = order.User.LastName,
            Username = order.User.Username,
            TotalAmount = order.TotalAmount,
            OrderCreatedOn = order.Created,
            Items = order.OrderItems.Select(item => new OrderItemDTO
            {
                ProductName = item.Product!.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Image = ImageDataUriBuilder.FromImage(item.Product.Image)
            }).ToList()
        }).ToList();


        response.Data = ordersDto;
        response.Status = ResponseStatus.Success;
        response.TotalCount = totalCount;
        return response;
    }

}
