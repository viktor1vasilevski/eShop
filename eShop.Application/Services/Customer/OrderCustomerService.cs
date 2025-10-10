using eShop.Application.Interfaces.Customer;
using eShop.Application.Requests.Customer.Order;
using eShop.Application.Responses.Customer.Order;
using eShop.Application.Responses.Shared.Base;
using eShop.Domain.Interfaces;
using eShop.Domain.Interfaces.Base;
using eShop.Domain.Models;
using System.Text;

namespace eShop.Application.Services.Customer;

public class OrderCustomerService(IUnitOfWork _uow) : IOrderCustomerService
{
    private readonly IEfRepository<Order> _orderRepository = _uow.GetEfRepository<Order>();
    private readonly IEfRepository<User> _userRepository = _uow.GetEfRepository<User>();
    private readonly IEfRepository<Product> _productRepository = _uow.GetEfRepository<Product>();

    public async Task<ApiResponse<List<OrderDetailsCustomerDto>>> GetOrdersForUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse<OrderDetailsCustomerDto>> PlaceOrderAsync(PlaceOrderCustomerRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
