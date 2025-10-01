using eShop.Application.DTOs.Order;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace eShop.Application.Services.Customer;

public class OrderCustomerService(IUnitOfWork _uow, ILogger<OrderCustomerService> _logger) : IOrderCustomerService
{
    private readonly IRepositoryBase<Order> _orderRepository = _uow.GetRepository<Order>();
    private readonly IRepositoryBase<User> _userRepository = _uow.GetRepository<User>();
    private readonly IRepositoryBase<Product> _productRepository = _uow.GetRepository<Product>();


    public async Task<ApiResponse<OrderDetailsDTO>> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var response = new ApiResponse<OrderDetailsDTO>();

        var user = _userRepository.GetById(request.UserId);
        if (user == null)
        {
            response.Status = ResponseStatus.NotFound;
            response.Message = "User not found.";
            return response;
        }

        var order = Order.Create(request.UserId);

        foreach (var itemRequest in request.Items)
        {
            var product = _productRepository.GetById(itemRequest.ProductId);
            if (product == null)
            {
                response.Status = ResponseStatus.NotFound;
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
