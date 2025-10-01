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

            productLines.Add((ProductName: product.Name ?? $"Product {product.Id}", Quantity: itemRequest.Quantity, UnitPrice: product.UnitPrice));
        }

        order.TotalValue(request.TotalAmount);

        await _orderRepository.InsertAsync(order);
        await _uow.SaveChangesAsync();

        response.Status = ResponseStatus.Success;

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
            _logger.LogWarning(ex, "Order {OrderId}: failed to enqueue confirmation email to {Email}", order.Id, user.Email);
        }

        return response;
    }

    private string BuildOrderHtml(Order order, User user, List<(string ProductName, int Quantity, decimal UnitPrice)> productLines)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\"/>");
        // small inline style block — Gmail supports simple styles inline or in head
        sb.AppendLine("<style>");
        sb.AppendLine("  table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("  th, td { padding: 8px; border: 1px solid #ddd; }");
        sb.AppendLine("  th { background-color: #f4f4f4; text-align: left; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h2>Thanks for your order, {System.Net.WebUtility.HtmlEncode(user.FirstName ?? user.Email)}!</h2>");
        sb.AppendLine($"<p>Order ID: <strong>{order.Id}</strong></p>");
        sb.AppendLine("<table role=\"presentation\" aria-hidden=\"true\">");
        sb.AppendLine("<thead><tr><th>Product</th><th style='text-align:center'>Qty</th><th style='text-align:right'>Price</th></tr></thead>");
        sb.AppendLine("<tbody>");
        foreach (var line in productLines)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"  <td>{System.Net.WebUtility.HtmlEncode(line.ProductName)}</td>");
            sb.AppendLine($"  <td style='text-align:center'>{line.Quantity}</td>");
            sb.AppendLine($"  <td style='text-align:right'>{line.UnitPrice:C}</td>");
            sb.AppendLine("</tr>");
        }
        sb.AppendLine("</tbody>");
        sb.AppendLine("</table>");
        sb.AppendLine($"<p><strong>Total: {order.TotalAmount:C}</strong></p>");
        sb.AppendLine("<p>We will notify you when your order ships.</p>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

}
