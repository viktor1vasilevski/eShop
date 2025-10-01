using eShop.Application.Common;
using eShop.Application.DTOs.Order;
using eShop.Application.Enums;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses;
using eShop.Domain.Entities;
using eShop.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace eShop.Application.Services.Customer;

public class OrderCustomerService : IOrderCustomerService
{
    private readonly IUnitOfWork _uow;
    private readonly IRepositoryBase<Order> _orderRepository;
    private readonly IRepositoryBase<User> _userRepository;
    private readonly IRepositoryBase<Product> _productRepository;
    private readonly IEmailQueue _emailQueue;
    private readonly ILogger<OrderCustomerService> _logger;

    public OrderCustomerService(IUnitOfWork uow,
                                ILogger<OrderCustomerService> logger,
                                IEmailQueue emailQueue)
    {
        _uow = uow;
        _logger = logger;
        _emailQueue = emailQueue ?? throw new ArgumentNullException(nameof(emailQueue));

        _orderRepository = _uow.GetRepository<Order>();
        _userRepository = _uow.GetRepository<User>();
        _productRepository = _uow.GetRepository<Product>(); // keep your original repo init
    }

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

        // We'll build a small list of lines for the email so we don't need extra DB reads later
        var productLines = new List<(string ProductName, int Quantity, decimal UnitPrice)>();

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

            // capture name/qty/price for the email
            productLines.Add((ProductName: product.Name ?? $"Product {product.Id}", Quantity: itemRequest.Quantity, UnitPrice: product.UnitPrice));
        }

        order.TotalValue(request.TotalAmount);

        await _orderRepository.InsertAsync(order);
        await _uow.SaveChangesAsync();

        // Build response DTO as you normally do
        response.Status = ResponseStatus.Success;


        // --- Build HTML and enqueue email (non-blocking work)
        try
        {
            var html = BuildOrderHtml(order, user, productLines);
            var subject = $"Order Confirmation #{order.Id}";

            var emailMessage = new EmailMessage(
                To: "viktorvasilevski2@gmail.com",
                Subject: subject,
                HtmlBody: html
            );

            await _emailQueue.EnqueueAsync(emailMessage);

            _logger.LogInformation("Order {OrderId}: email enqueued to {Email}", order.Id, user.Email);
        }
        catch (Exception ex)
        {
            // Never fail the order because email enqueue failed; just log it.
            _logger.LogWarning(ex, "Order {OrderId}: failed to enqueue confirmation email to {Email}", order.Id, user.Email);
        }

        return response;
    }

    private string BuildOrderHtml(Order order, User user, List<(string ProductName, int Quantity, decimal UnitPrice)> productLines)
    {
        var sb = new StringBuilder();
        sb.Append($"<h2>Thanks for your order, {System.Net.WebUtility.HtmlEncode(user.FirstName ?? user.Email)}!</h2>");
        sb.Append($"<p>Order ID: <strong>{order.Id}</strong></p>");
        sb.Append("<table style='border-collapse:collapse;width:100%'>");
        sb.Append("<thead><tr><th style='text-align:left;padding:8px'>Product</th><th style='padding:8px'>Qty</th><th style='text-align:right;padding:8px'>Price</th></tr></thead>");
        sb.Append("<tbody>");
        foreach (var line in productLines)
        {
            sb.Append("<tr>");
            sb.Append($"<td style='padding:8px'>{System.Net.WebUtility.HtmlEncode(line.ProductName)}</td>");
            sb.Append($"<td style='text-align:center;padding:8px'>{line.Quantity}</td>");
            sb.Append($"<td style='text-align:right;padding:8px'>{line.UnitPrice:C}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody>");
        sb.Append("</table>");
        sb.Append($"<p><strong>Total: {order.TotalAmount:C}</strong></p>");
        sb.Append("<p>We will notify you when your order ships.</p>");
        return sb.ToString();
    }
}
