using eShop.Application.DTOs.Order;
using eShop.Application.DTOs.OrderItem;
using eShop.Application.Enums;
using eShop.Application.Extensions;
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
    private readonly IRepositoryBase<OrderItem> _orderItemRepository = _uow.GetRepository<OrderItem>();
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();

    public ApiResponse<List<OrderDetailsDTO>> GetOrders(OrderRequest request)
    {
        var query = _orderRepository.GetAsQueryableWhereIf(
            filter: x => x.WhereIf(request.UserId != Guid.Empty, o => o.UserId == request.UserId),
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
            }).ToList()
        }).ToList();
       
        return new ApiResponse<List<OrderDetailsDTO>>
        {
            Data = ordersDTO,
            NotificationType = NotificationType.Success,
        };
    }

    public async Task<ApiResponse<OrderDetailsDTO>> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var response = new ApiResponse<OrderDetailsDTO>();

        var user = _userRepository.GetById(request.UserId);
        if (user == null)
        {
            response.NotificationType = NotificationType.NotFound;
            response.Message = "User not found.";
            return response;
        }

        var order = Order.Create(request.UserId);

        foreach (var itemRequest in request.Items)
        {
            var product = _productRepository.GetById(itemRequest.ProductId);
            if (product == null)
            {
                response.NotificationType = NotificationType.NotFound;
                response.Message = $"Product with ID {itemRequest.ProductId} not found.";
                return response;
            }

            product.SubtrackQuantity(itemRequest.Quantity);

            var orderItem = OrderItem.Create(product.Id, itemRequest.Quantity, product.UnitPrice);
            order.AddOrderItem(orderItem);
        }

        order.TotalValue(request.TotalAmount);

        await _orderRepository.InsertAsync(order);
        await _uow.SaveChangesAsync();

        return response;
    }

}
