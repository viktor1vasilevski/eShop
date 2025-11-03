using eShop.Application.Constants.Customer;
using eShop.Application.Enums;
using eShop.Application.Helpers;
using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses.Customer.Order;
using eShop.Application.Responses.Customer.OrderItem;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces.EntityFramework;
using eShop.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace eShop.Application.Services.Customer;

public class OrderCustomerService(IEfUnitOfWork _uow, IEfRepository<Order> _orderRepository, IEfRepository<User> _userRepository, IEfRepository<Product> _productRepository, 
    ILogger<OrderCustomerService> _logger, IEmailQueue _emailQueue) : IOrderCustomerService
{

    public async Task<ApiResponse<List<OrderDetailsCustomerResponse>>> GetOrdersForUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var (query, totalCount) = await _orderRepository.QueryAsync(
            queryBuilder: x => x.Where(a => a.UserId == userId),
            orderBy: x => x.OrderByDescending(x => x.Created),
            includeBuilder: x => x.Include(oi => oi.OrderItems).ThenInclude(p => p.Product),
            selector: x => new OrderDetailsCustomerResponse
                {
                    FirstName = x.User.FullName.FirstName,
                    LastName = x.User.FullName.LastName,
                    Username = x.User.Username.Value,
                    TotalAmount = x.TotalAmount,
                    OrderCreatedOn = x.Created,
                    Items = x.OrderItems.Select(item => new OrderItemCustomerDto
                    {
                        ProductName = item.Product!.Name.Value,
                        Quantity = item.UnitQuantity.Value,
                        UnitPrice = item.UnitPrice.Value,
                        Image = ImageDataUriBuilder.FromImage(item.Product.Image)
                    }).ToList()
                }
            );

        if (query is null || totalCount == 0)
        {
            return new ApiResponse<List<OrderDetailsCustomerResponse>>
            {
                Data = [],
                Status = ResponseStatus.Success,
            };
        }

        return new ApiResponse<List<OrderDetailsCustomerResponse>>
        {
            Data = query.ToList(),
            Status = ResponseStatus.Success,
            TotalCount = totalCount,
        };
    }

    public async Task<ApiResponse<OrderDetailsCustomerResponse>> PlaceOrderAsync(PlaceOrderCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetSingleAsync(
            filter: u => u.Id == request.UserId,
            selector: s => s,
            cancellationToken: cancellationToken);

        if (user is null)
        {
            return new ApiResponse<OrderDetailsCustomerResponse>
            {
                Status = ResponseStatus.NotFound,
                Message = CustomerAuthConstants.UserNotFound
            };
        }

        var order = Order.Create(request.UserId);
        var productLines = new List<(string ProductName, int Quantity, decimal UnitPrice)>();

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var (products, _) = await _productRepository.QueryAsync(
            queryBuilder: q => q.Where(p => productIds.Contains(p.Id)),
            selector: p => p,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        var productDict = products.ToDictionary(p => p.Id);

        foreach (var itemRequest in request.Items)
        {
            if (!productDict.TryGetValue(itemRequest.ProductId, out var product))
            {
                return new ApiResponse<OrderDetailsCustomerResponse>
                {
                    Status = ResponseStatus.NotFound,
                    Message = string.Format(CustomerOrderConstants.ProductNotFound, itemRequest.ProductId)
                };
            }

            product.SubtrackQuantity(itemRequest.Quantity);

            var orderItem = OrderItem.Create(product.Id, itemRequest.Quantity, product.UnitPrice.Value);
            order.AddOrderItem(orderItem);

            productLines.Add((
                ProductName: product.Name.Value ?? $"Product {product.Id}",
                Quantity: itemRequest.Quantity,
                UnitPrice: product.UnitPrice.Value
            ));
        }

        order.TotalValue(request.TotalAmount);

        await _orderRepository.AddAsync(order, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        try
        {
            var html = BuildOrderHtml(order, user, productLines);
            var subject = $"Order Confirmation #{order.Id}";

            var emailMessage = new EmailMessage(
                To: user.Email.Value,
                Subject: subject,
                HtmlBody: html);

            await _emailQueue.EnqueueAsync(emailMessage);
            _logger.LogInformation("Order {OrderId}: confirmation email queued for {Email}", order.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Order {OrderId}: failed to queue confirmation email for {Email}", order.Id, user.Email);
        }

        return new ApiResponse<OrderDetailsCustomerResponse>
        {
            Status = ResponseStatus.Success,
            Message = CustomerOrderConstants.OrderPlaced,
            Data = new OrderDetailsCustomerResponse
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                OrderCreatedOn = order.Created,
                Items = productLines.Select(p => new OrderItemCustomerDto
                {
                    ProductName = p.ProductName,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice
                }).ToList()
            }
        };
    }


    private string BuildOrderHtml(Order order, User user, List<(string ProductName, int Quantity, decimal UnitPrice)> productLines)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\"/>");
        sb.AppendLine("<style>");
        sb.AppendLine("  table { border-collapse: collapse; width: 100%; }");
        sb.AppendLine("  th, td { padding: 8px; border: 1px solid #ddd; }");
        sb.AppendLine("  th { background-color: #f4f4f4; text-align: left; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h2>Thanks for your order, {System.Net.WebUtility.HtmlEncode(user.FullName.FirstName ?? user.Email.Value)}!</h2>");
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
