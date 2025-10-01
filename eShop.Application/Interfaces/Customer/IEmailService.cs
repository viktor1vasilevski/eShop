namespace eShop.Application.Interfaces.Customer;

public interface IEmailService
{
    Task SendHtmlAsync(string to, string subject, string html);
}
