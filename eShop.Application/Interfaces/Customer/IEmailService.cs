namespace eShop.Application.Interfaces.Customer;

public interface IEmailService
{
    Task SendOrderConfirmationAsync(string to, string orderId, string productName, decimal total);
    Task SendHtmlAsync(string to, string subject, string html);

}
